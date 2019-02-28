using System;
using System.Collections.Generic;
using C5;

namespace solution
{
    class Program
    {
        static int V,E,R,C,X;
        static List<int> S = new List<int>();
        static int [,]LAT_E_TO_C = new int [1005,1005];
        static Dictionary<int, List<int> > EDGES_E_TO_C = new Dictionary<int, List<int> >();
        static Dictionary<int, List<int> > EDGES_E_TO_V = new Dictionary<int, List<int> >();
        static int []LAT_E_TO_D = new int [1005];
        static int [,]REQ = new int[1000005,3];
        static bool [,]MAY_NEED_VIDEO = new bool[1005,10005];
        static int [,]ENDPOINT_TO_VIDEO_REQUESTS = new int[1005,10005];
        static long MAXSCORE;
        static long TOTALREQUESTS = 0L;
        static List<C5.HashSet<int>> Assignment = new List<C5.HashSet<int>>();
        static List<C5.HashSet<int>> BestAssignment = new List<C5.HashSet<int>>();
        static long []SpaceUsed= new long[1005];
        static long BestScore = 0L;
        static Random Random = new Random();
        static C5.IPriorityQueue<Tuple<double,int,int>> sortedLatencyGains = new IntervalHeap<Tuple<double,int,int>>();
        static C5.IPriorityQueueHandle<Tuple<double, int, int>>[,] latencyGainHandles = new C5.IPriorityQueueHandle<Tuple<double, int, int>>[1005,10005];

        static void Main(string[] args)
        {
            Initialize();
            ReadInput();
            Prefill();

            AddAllVideos();
            MAXSCORE = CalculateScore();
            Console.Out.WriteLine("Max theoretical score: {0}", 1000.0*MAXSCORE/TOTALREQUESTS);
            //PrintAssignment(Assignment);

            var iter = 0;
            while(iter<1){
                iter++;

                for(int c=0;c<C;c++){
                    Assignment[c].Clear();
                    SpaceUsed[c] = 0;
                }
                var score = Solve();
                if(score > BestScore){
                    Console.Out.WriteLine("Best Score: {0}, Previous: {1}", (1000.0*score/TOTALREQUESTS), (1000.0*BestScore/TOTALREQUESTS));
                    BestScore = score;
                    BestAssignment = Assignment;
                    PrintAssignment(BestAssignment);
                }
            }
        }

        private static void PrintAssignment(List<C5.HashSet<int>> assignment)
        {
            for(int c=0;c<assignment.Count;c++){
                Console.Out.Write(c);
                foreach(var vid in assignment[c]){
                    Console.Out.Write(" {0}",vid);
                }
                Console.Out.WriteLine();
            }
        }

        private static void Initialize()
        {
            for(int i=0;i<1005;i++)
            for(int j=0;j<1005;j++)
                LAT_E_TO_C[i,j] = int.MaxValue;
        }

        private static IEnumerable<int> Shuffle(List<int> list){
            for(var i=0;i<list.Count;i++){
                if(i==list.Count-1){
                    yield return list[i];
                } else{
                    int rem = list.Count - i;
                    var val = i + Random.Next()%rem;
                    yield return list[val];

                    var tmp = list[val];
                    list[val] = list[i];
                    list[i] = tmp;
                }
            }
            yield break;
        }
        private static long Solve()
        {
            Console.Out.Write("Initial generation of latency gains... ");

            long [,]latencyGains = new long[C,V];

            for(var v=0;v<V;v++){
                if(S[v] > X) continue;
                for(var c=0;c<C;c++){
                    var totalLatencyGain = 0L;
                    for(var e=0;e<E;e++){
                        if(LAT_E_TO_C[e,c] == int.MaxValue) continue;
                        var numRequests = ENDPOINT_TO_VIDEO_REQUESTS[e,v];
                        if(numRequests == 0) continue;
                        var latencyGain = (long)(LAT_E_TO_D[e] - LAT_E_TO_C[e,c]) * numRequests;
                        totalLatencyGain += latencyGain;
                    }
                    C5.IPriorityQueueHandle<Tuple<double, int, int>> handle = null;
                    sortedLatencyGains.Add(ref handle, new Tuple<double, int, int>(1.0*totalLatencyGain/S[v], c, v));
                    latencyGainHandles[c,v] = handle;
                    latencyGains[c,v] = totalLatencyGain;
                }
            }
            Console.Out.WriteLine("Done!");

            while(!sortedLatencyGains.IsEmpty){
                var best = sortedLatencyGains.FindMax();
                sortedLatencyGains.DeleteMax();
                //Console.Out.WriteLine("--> gain: "+best.Item1+" c: "+best.Item2+" v: "+best.Item3);
                var gain = best.Item1;
                var c = best.Item2;
                var v = best.Item3;

                if(S[v] + SpaceUsed[c] > X) continue;
                Assignment[c].Add(v);
                SpaceUsed[c] += S[v];

                for(var cx=0;cx<C;cx++)
                    latencyGains[cx,v] = 0L;

                for(var ex=0;ex<E;ex++){
                    var bestCache = -1;
                    var bestLatency = LAT_E_TO_D[ex];

                    foreach(var cx in EDGES_E_TO_C[ex]){
                        if(!Assignment[cx].Contains(v)) continue;
                        if(LAT_E_TO_C[ex, cx] < bestLatency){
                            bestCache = cx;
                            bestLatency = LAT_E_TO_C[ex, cx];
                        }
                    }

                    foreach(var cx in EDGES_E_TO_C[ex]){
                        if(Assignment[cx].Contains(v)) continue;
                        if(LAT_E_TO_C[ex, cx] < bestLatency){
                            latencyGains[cx, v] += (bestLatency - LAT_E_TO_C[ex,cx]) * ENDPOINT_TO_VIDEO_REQUESTS[ex, v];
                        }
                    }
                }

                for(var cx=0;cx<C;cx++){
                    var handle = latencyGainHandles[cx, v];
                    Tuple<double, int, int> item;
                    if(sortedLatencyGains.Find(handle, out item)){
                        sortedLatencyGains.Delete(handle);
                        sortedLatencyGains.Add(ref handle, new Tuple<double, int, int>(1.0*latencyGains[cx, v]/S[v], cx, v));
                    }
                }
            }

            VerifySolution();
            return CalculateScore();
        }

        private static void Prefill(){
            Console.Out.WriteLine("Prefilling values");
            for(int c=0;c<C;c++){
                Assignment.Add(new C5.HashSet<int>());
            }

            for(int r=0;r<R;r++){
                int v = REQ[r,0];
                int e = REQ[r,1];
                int n = REQ[r,2];
                ENDPOINT_TO_VIDEO_REQUESTS[e,v] = n;
                TOTALREQUESTS += n;
                foreach(var c in EDGES_E_TO_C[e]){
                    MAY_NEED_VIDEO[c, v] = true;
                }
            }
        }
        private static void AddAllVideos()
        {
            for(int c=0;c<C;c++)
                for(int v=0;v<V;v++)
                    if(MAY_NEED_VIDEO[c, v])
                        Assignment[c].Add(v);
        }

        private static long CalculateScore(){
            long score = 0;

            for(int r=0;r<R;r++){
                int v = REQ[r,0];
                int e = REQ[r,1];
                int n = REQ[r,2];

                long savings = 0;
                int bestCache = -1;
                foreach(var c in EDGES_E_TO_C[e]){
                    if(Assignment[c].Contains(v)){
                        var curSavings = (long)(LAT_E_TO_D[e] - LAT_E_TO_C[e,c]) * n ;
                        if(curSavings > savings){
                            savings = curSavings;
                            bestCache = c;
                        }
                    }
                }

                score += savings;
            }

            return score;
        }

        private static bool VerifySolution(){
            for(int c=0;c<C;c++){
                var spaceUsed = 0;
                foreach(var v in Assignment[c]){
                    spaceUsed += S[v];
                }
                if(spaceUsed > X){
                    throw new Exception("Invalid solution!");
                }
            }
            return true;
        }

        static void ReadInput(){
            var inp = Console.ReadLine().Split(' '); int t = 0;
            V = int.Parse(inp[t++]);
            E = int.Parse(inp[t++]);
            R = int.Parse(inp[t++]);
            C = int.Parse(inp[t++]);
            X = int.Parse(inp[t++]);

            for(int e=0;e<E;e++)
                EDGES_E_TO_C[e] = new List<int>();

            inp = Console.ReadLine().Split(' ');
            foreach(var token in inp){
                S.Add(int.Parse(token));
            }

            for(int e=0;e<E;e++){
                inp = Console.ReadLine().Split(' '); t = 0;
                int LD = int.Parse(inp[t++]);
                int K = int.Parse(inp[t++]);

                LAT_E_TO_D[e] = LD;
                for(int k=0;k<K;k++){
                    inp = Console.ReadLine().Split(' '); t = 0;
                    int cid = int.Parse(inp[t++]);
                    int l = int.Parse(inp[t++]);
                    LAT_E_TO_C[e,cid] = l;
                    EDGES_E_TO_C[e].Add(cid);
                }
            }

            for(int r=0;r<R;r++){
                inp = Console.ReadLine().Split(' '); t = 0;
                int v = int.Parse(inp[t++]);
                int e = int.Parse(inp[t++]);
                int n = int.Parse(inp[t++]);
                REQ[r, 0] = v;
                REQ[r, 1] = e;
                REQ[r, 2] = n;
            }
        }
    }

}