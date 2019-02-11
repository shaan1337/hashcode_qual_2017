using System;
using System.Collections.Generic;

namespace solution
{
    class Program
    {
        static int V,E,R,C,X;
        static List<int> S = new List<int>();
        static int [,]LAT_E_TO_C = new int [1005,1005];
        static Dictionary<int, List<int> > EDGES_E_TO_C = new Dictionary<int, List<int> >();
        static int []LAT_E_TO_D = new int [1005];
        static int [,]REQ = new int[1000005,3];
        static bool [,]MAY_NEED_VIDEO = new bool[1005,10005];
        static long MAXSCORE;
        static long TOTALREQUESTS = 0L;

        static List<HashSet<int>> Assignment = new List<HashSet<int>>();
        static List<HashSet<int>> BestAssignment = new List<HashSet<int>>();
        static long []SpaceUsed= new long[1005];
        static long BestScore = 0L;
        static Random Random = new Random();

        static void Main(string[] args)
        {
            Initialize();
            ReadInput();
            Prefill();

            AddAllVideos();
            MAXSCORE = CalculateScore();            
            Console.Out.WriteLine("Max theoretical score: {0}", 1000.0*MAXSCORE/TOTALREQUESTS);
            //PrintAssignment(Assignment);

            while(true){
                AddAllVideos();
                var score = Solve();
                if(score > BestScore){
                    Console.Out.WriteLine("Best Score: {0}, Previous: {1}", (1000.0*score/TOTALREQUESTS), (1000.0*BestScore/TOTALREQUESTS));
                    BestScore = score;
                    BestAssignment = Assignment;
                    //PrintAssignment(BestAssignment);
                }
            }
        }

        private static void PrintAssignment(List<HashSet<int>> assignment)
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

        private static long Solve()
        {            
            List<int> overloaded = new List<int>();
            for(int c=0;c<C;c++){
                long spaceUsed = 0L;
                foreach(var vid in Assignment[c]){
                    spaceUsed += S[vid];
                }
                if(spaceUsed > X){
                    overloaded.Add(c);
                }
                SpaceUsed[c] = spaceUsed;
            }

            while(overloaded.Count > 0){
                int cid = overloaded[Random.Next()%overloaded.Count];
                var videos = new List<int>(Assignment[cid]);
                var vid = videos[Random.Next()%videos.Count];
                Assignment[cid].Remove(vid);
                SpaceUsed[cid] -= S[vid];
                if(SpaceUsed[cid] <= X){
                    overloaded.Remove(cid);
                }
            }

            return CalculateScore();
        }

        private static void Prefill(){
            Console.Out.WriteLine("Prefilling values");
            for(int c=0;c<C;c++){
                Assignment.Add(new HashSet<int>());
            }

            for(int r=0;r<R;r++){
                int v = REQ[r,0];
                int e = REQ[r,1];
                int n = REQ[r,2];
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
