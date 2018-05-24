using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peacenet.WorldGenerator
{
    public class Heightmap
    {
        private int _width = 1024;
        private int _height = 1024;
        private int _minFeatures = 100;
        private int _maxFeatures = 150;
        private double _minHeight = 0.6;
        private double _maxHeight = 1;
        private int _minRadius = 64;
        private int _maxRadius = 90;
        private double _voidLevel = 2.75;

        public int Width { get => _width; set => _width = value; }
        public int Height { get => _height; set => _height = value; }
        public int MinimumFeatures { get => _minFeatures; set => _minFeatures = value; }
        public int MaximumFeatures { get => _maxFeatures; set => _maxFeatures = value; }
        public double MinHeightValue { get => _minHeight; set => _minHeight = value; }
        public double MaxHeightValue { get => _maxHeight; set => _maxHeight = value; }
        public int MinRadius { get => _minRadius; set => _minRadius = value; }
        public int MaxRadius { get => _maxRadius; set => _maxRadius = value; }
        public double VoidLevel { get => _voidLevel; set => _voidLevel = value; }

        private Random _rnd = null;
        private int _seed = 0;

        public int Seed => _seed;

        /// <summary>
        /// Create a new instance of the <see cref="Heightmap"/> class, with a random seed.
        /// </summary>
        public Heightmap()
        {
            _seed = new Random().Next(int.MinValue, int.MaxValue);
            _rnd = new Random(_seed);

            
        }

        /// <summary>
        /// Create a new instance of the <see cref="Heightmap"/> class with the specified integer seed.
        /// </summary>
        /// <param name="seed">The seed to use during heightmap generation.</param>
        public Heightmap(int seed)
        {
            _seed = seed;
            _rnd = new Random(_seed);
        }

        /// <summary>
        /// Generates a heightmap using the integer seed value and the specified heightmap parameters.
        /// </summary>
        /// <returns>The generated heightmap, as an array of percentage values.</returns>
        public double[] Generate()
        {
            var map = new double[_width * _height];
            var nFeatures = _rnd.Next(_minFeatures, _maxFeatures + 1);
            for(int i = 0; i < nFeatures;i++)
            {
                //Get the peak height of this landscape feature, making sure it is between our minimum and maximum height values.
                var height = _rnd.NextDouble() * (_maxHeight - _minHeight) + _minHeight;
                //Get the radius of this landscape feature.
                var radius = _rnd.Next(_minRadius, _maxRadius);
                //Get the center coordinates of the landmass.
                var cX = _rnd.Next(_width);
                var cY = _rnd.Next(_height);

                //Now's the fun part.
                for (var y = Math.Max(0, cY - radius); y < Math.Min(cY + radius, Height); y++)
                    for (var x = Math.Max(0, cX - radius); x < Math.Min(cX + radius, Width); x++)
                        map[y * Width + x] += height * Math.Max(0, (radius - Math.Sqrt(Math.Pow(x - cX, 2) + Math.Pow(y - cY, 2))) / radius);


            }

            //Now we gotta get the min and max values found in the map.
            double max = 0, min = 0;
            foreach (var val in map)
            {
                if (val > max)
                    max = val;
                if (val < min)
                    min = val;
            }

            //Now we'll normalize the values.
            for (int i = 0; i < map.Length; i++)
            {
                double v = map[i];
                //If the value's below the void level we'll make the percentage 0.
                if (v < _voidLevel)
                    map[i] = 0;
                //Else, we'll calculate the percentage of the height.
                else
                {
                    double percentage = (v - min) / (max - min);
                    map[i] = percentage;
                }
            }

            //Done generation.
            return map;
        }

        /// <summary>
        /// Converts a string seed to an integer seed.
        /// </summary>
        /// <param name="value">The string seed value to convert.</param>
        /// <returns>The converted integer seed.</returns>
        public static int GetSeed(string value)
        {
            int s = 0;
            if(!int.TryParse(value, out s))
            {
                for (int i = 0; i < value.Length; i++)
                {
                    int charCode = (int)value[i];
                    if ((i % 2) == 0)
                    {
                        s += -charCode * (int)Math.Pow(i, i / 2);
                    }
                    else
                    {
                        s += (int)Math.Pow(charCode, i);
                    }
                }
            }
            return s;
        }
    }
}
