#region Copyright & licence

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
// License along with Refix.  If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 David Musgrove and others.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;

namespace NinjaTurtles.Reporting
{
    [Serializable]
    public class SequencePoint
    {
        private readonly ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();

        public SequencePoint()
        {
            AppliedMutants = new List<AppliedMutant>();
            _readerWriterLock = new ReaderWriterLockSlim();
        }

        public SequencePoint(Mono.Cecil.Cil.SequencePoint sequencePoint)
            : this()
        {
            StartLine = sequencePoint.StartLine;
            StartColumn = sequencePoint.StartColumn;
            EndLine = sequencePoint.EndLine;
            EndColumn = sequencePoint.EndColumn;
        }

        public SequencePoint(SequencePoint sequencePoint)
            : this()
        {
            StartLine = sequencePoint.StartLine;
            StartColumn = sequencePoint.StartColumn;
            EndLine = sequencePoint.EndLine;
            EndColumn = sequencePoint.EndColumn;
        }

        [XmlAttribute]
        public int StartLine { get; set; }

        [XmlAttribute]
        public int StartColumn { get; set; }

        [XmlAttribute]
        public int EndLine { get; set; }

        [XmlAttribute]
        public int EndColumn { get; set; }

        public string GetIdentifier()
        {
            return GetIdentifier(StartLine, StartColumn, EndLine, EndColumn);
        }
        
        static public string GetIdentifier(Mono.Cecil.Cil.SequencePoint sequencePoint)
        {
            return GetIdentifier(sequencePoint.StartLine, sequencePoint.StartColumn, sequencePoint.EndLine,
                                 sequencePoint.EndColumn);
        }

        static public string GetIdentifier(int startLine, int startColumn, int endLine, int endColumn)
        {
            return string.Format("{0}_{1}_{2}_{3}", startLine, startColumn, endLine, endColumn);
        }

        public List<AppliedMutant> AppliedMutants { get; set; }

        public void AddResult(MutationTestMetaData mutationTestMetaData, bool mutantKilled)
        {
            _readerWriterLock.EnterUpgradeableReadLock();
            try
            {
                if (!AppliedMutants.Any(s => s.Description == mutationTestMetaData.Description))
                {
                    _readerWriterLock.EnterWriteLock();
                    AppliedMutants.Add(new AppliedMutant
                                             {
                                                 Description = mutationTestMetaData.Description,
                                                 Killed = mutantKilled
                                             });
                    _readerWriterLock.ExitWriteLock();
                }
            }
            finally
            {
                _readerWriterLock.ExitUpgradeableReadLock();
            }
        }

        public void MergeFrom(SequencePoint sequencePoint)
        {
            foreach (var appliedMutant in sequencePoint.AppliedMutants)
            {
                if (!AppliedMutants.Any(a => a.Description == appliedMutant.Description))
                {
                    AppliedMutants.Add(appliedMutant);
                }
            }
        }
    }
}
