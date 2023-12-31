﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WorldMapStrategyKit.MapGenerator.Geom {
	class PointChain {
		public bool closed;
		public List<Point> pointList;

		//		static int maxLen = 0;

		public PointChain (Segment s) {
			pointList = new List<Point> (50);
			pointList.Add (s.start);
			pointList.Add (s.end);
			closed = false;
		}
		
		// Links a segment to the pointChain
		public bool LinkSegment (Segment s) {
			Point front = pointList [0];
			Point back = pointList [pointList.Count - 1];
			
			if (Point.EqualsBoth (s.start, front)) {
				if (Point.EqualsBoth (s.end, back)) {
					closed = true;
				} else {
					pointList.Insert (0, s.end);
				}
				return true;
			} else if (Point.EqualsBoth (s.end, back)) {
				if (Point.EqualsBoth (s.start, front)) {
					closed = true;
				} else {
					pointList.Add (s.start);
				}
				return true;
			} else if (Point.EqualsBoth (s.end, front)) {
				if (Point.EqualsBoth (s.start, back)) {
					closed = true;
				} else {
					pointList.Insert (0, s.start);
				}
				return true;
			} else if (Point.EqualsBoth (s.start, back)) {
				if (Point.EqualsBoth (s.end, front)) {
					closed = true;
				} else {
					pointList.Add (s.end);
				}
				return true;
			}
			
			return false;
		}
		
		// Links another pointChain onto this point chain.
		public bool LinkPointChain (PointChain chain) {
			Point firstPoint = pointList [0];
			Point lastPoint = pointList [pointList.Count - 1];
			
			Point chainFront = chain.pointList [0];
			Point chainBack = chain.pointList [chain.pointList.Count - 1];
			
			if (Point.EqualsBoth (chainFront, lastPoint)) {
				int chainPointListCount = chain.pointList.Count;
				List<Point> temp = new List<Point> (chainPointListCount);
				for (int k = 1; k < chainPointListCount; k++) {
					temp.Add (chain.pointList [k]);
				}
				pointList.AddRange (temp);
				return true;
			}
			
			if (Point.EqualsBoth (chainBack, firstPoint)) {
				List<Point> temp = new List<Point> (chain.pointList);
				int pointListCount = pointList.Count;
				temp.Capacity += pointListCount;
				for (int k = 1; k < pointListCount; k++) {
					temp.Add (pointList [k]);
				}
				pointList = temp;
				return true;
			}
			
			if (Point.EqualsBoth (chainFront, firstPoint)) {
				List<Point> temp = new List<Point> (chain.pointList);
				temp.Reverse ();
				int pointListCount = pointList.Count;
				temp.Capacity += pointListCount;
				for (int k = 1; k < pointListCount; k++) {
					temp.Add (pointList [k]);
				}
				pointList = temp;
				return true;
			}
			
			if (Point.EqualsBoth (chainBack, lastPoint)) {
				pointList.RemoveAt (pointList.Count - 1);
				List<Point> temp = new List<Point> (chain.pointList);
				temp.Reverse ();
				pointList.AddRange (temp);
				return true;
			}
			return false;
		}
		
	}

}