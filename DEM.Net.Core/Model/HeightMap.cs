﻿// HeightMap.cs
//
// Author:
//       Xavier Fischer 
//
// Copyright (c) 2019 
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEM.Net.Core
{
    [Serializable()]
    public class HeightMap 
    {
        public HeightMap(int width, int height)
        {
            Width = width;
            Height = height;
            Coordinates = null;
            Count = width * height;
        }

        private BoundingBox _bbox;
        public BoundingBox BoundingBox
        {
            get
            {
                if (_bbox == null)
                {
                    Logger.Info("Computing bbox...");
                    _bbox = new BoundingBox(Coordinates.Min(c => c.Longitude)
                        , Coordinates.Max(c => c.Longitude)
                        , Coordinates.Min(c => c.Latitude)
                        , Coordinates.Max(c => c.Latitude));
                }
                return _bbox;
            }
            set
            {
                _bbox = value;
            }
        }

        public IEnumerable<GeoPoint> Coordinates { get; set; }

        /// <summary>
        /// Coordinate count
        /// </summary>
        public int Count { get; set; }

        public float Mininum { get; set; }
        public float Maximum { get; set; }
        public float Range
        {
            get { return Maximum - Mininum; }
        }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public HeightMap Clone()
        {
            return (HeightMap)this.MemberwiseClone();
        }

    }
}
