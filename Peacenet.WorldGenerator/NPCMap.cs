using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peacenet.WorldGenerator
{
    public class NPCMap
    {
        private int _seed = 0;
        private Random _rnd = null;

        private int _minimumStoryNPCs = 0;
        private bool _spawnStoryNPCs = false;
        private double[] _map = null;
        private int _maxDensity = 64;
        private float _chance = 0.1f;

        public bool SpawnStoryNPCs
        {
            get
            {
                return _spawnStoryNPCs;
            }
            set
            {
                _spawnStoryNPCs = value;
            }
        }

        public int MinStoryNPCs
        {
            get
            {
                return _minimumStoryNPCs;
            }
            set
            {
                _minimumStoryNPCs = value;
            }
        }


        public NPCMap(int seed, double[] heightmap)
        {
            _map = heightmap;
            _seed = seed;
            _rnd = new Random(_seed);
        }

        public bool[] GenerateDensityMap()
        {
            bool[] map = new bool[_map.Length];
            for(int i = 0; i < map.Length; i++)
            {
                double p = _map[i];
                double d = p * _maxDensity;
                double c = _rnd.NextDouble() * _maxDensity;
                if(c < d * _chance)
                {
                    map[i] = true;
                }
            }
            return map;
        }

        public int[] GetTypeMap(bool[] density)
        {
            //NPC types:
            //0: no npc
            //1: story slot
            //2: malicious singular
            //3: malicious faction
            //4: neutral singular
            //5: neutral faction
            //6: heroic singular
            //7: heroic faction
            //8: player spawn

            int[] map = new int[density.Length];
            List<int> indexes = new List<int>();
            for(int i = 0; i < map.Length; i++)
            {
                if (!density[i])
                    continue;

                indexes.Add(i);
            }

            bool playerSpawned = false;

            while (indexes.Count > 0)
            {
                int i = _rnd.Next(indexes.Count);
                int index = indexes[i];
                indexes.RemoveAt(i);

                if(_spawnStoryNPCs)
                {
                    if(_minimumStoryNPCs>0)
                    {
                        map[index] = 1;
                        _minimumStoryNPCs--;
                        continue;
                    }

                    if(playerSpawned==false)
                    {
                        map[index] = 8;
                        playerSpawned = true;
                        continue;
                    }
                }



                if (_rnd.NextDouble() > _chance)
                    continue;

                int type = _rnd.Next(6)+2;
                map[index] = type;
#if DEBUG
                Console.WriteLine($"map[{index}] = {type}");
#endif
            }

#if DEBUG
            Console.WriteLine($"map[singular] = {map.Where(x => x % 2 == 0).Count()}");
#endif

            return map;
        }

        


    }
}
