﻿#region Copyright & licence

// This file is part of NinjaTurtles.
// 
// NinjaTurtles is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// NinjaTurtles is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with NinjaTurtles.  If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 David Musgrove and others.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace NinjaTurtles.Reporting
{
    [Serializable]
    internal class SourceFile
    {
        private readonly ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();

        public SourceFile()
        {
            SequencePoints = new List<SequencePoint>();
            _readerWriterLock = new ReaderWriterLockSlim();
            Lines = new List<Line>();
        }

        public string Url { get; set; }

        public void SetUrl(string url)
        {
            Url = url;
            if (File.Exists(url))
            {
                var lines = File.ReadAllLines(url);
                Lines.Clear();
                for (int i = 0; i < lines.Length; i++)
                {
                    Lines.Add(new Line { Text = lines[i].Replace("\t", "    "), Number = i + 1 });
                }
            }
        }

        public List<SequencePoint> SequencePoints { get; set; }

        public List<Line> Lines { get; set; } 

        public void AddResult(Mono.Cecil.Cil.SequencePoint sequencePoint, MutationTestMetaData mutationTestMetaData, bool mutantKilled)
        {
            string identifier = SequencePoint.GetIdentifier(sequencePoint);
            _readerWriterLock.EnterUpgradeableReadLock();
            try
            {
                if (!SequencePoints.Any(s => s.GetIdentifier() == identifier))
                {
                    _readerWriterLock.EnterWriteLock();
                    SequencePoints.Add(new SequencePoint(sequencePoint));
                    _readerWriterLock.ExitWriteLock();
                }
            }
            finally
            {
                _readerWriterLock.ExitUpgradeableReadLock();
            } 
            var sourceSequencePoint = SequencePoints.First(s => s.GetIdentifier() == identifier);
            sourceSequencePoint.AddResult(mutationTestMetaData, mutantKilled);
        }

        public void MergeFrom(SourceFile sourceFile)
        {
            foreach (var sequencePoint in sourceFile.SequencePoints)
            {
                if (!SequencePoints.Any(s => s.GetIdentifier() == sequencePoint.GetIdentifier()))
                {
                    SequencePoints.Add(sequencePoint);
                }
                else
                {
                    SequencePoints.First(s => s.GetIdentifier() == sequencePoint.GetIdentifier()).MergeFrom(
                        sequencePoint);
                }
            }
        }

        public void AddSequencePoint(Mono.Cecil.Cil.SequencePoint point)
        {
            if (!SequencePoints.Any(s => s.GetIdentifier() == SequencePoint.GetIdentifier(point)))
            {
                SequencePoints.Add(new SequencePoint(point));
            }
        }
    }
}
