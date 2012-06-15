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
using System.Xml.Serialization;

using Mono.Cecil;

namespace NinjaTurtles.Reporting
{
    [Serializable]
    internal class MutationTestingReport
    {
        private readonly ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();

        public MutationTestingReport()
        {
            SourceFiles = new List<SourceFile>();
            _readerWriterLock = new ReaderWriterLockSlim();
        }

        public List<SourceFile> SourceFiles { get; set; }

        public void MergeFromFile(string fileName)
        {
            if (!File.Exists(fileName)) return;

            MutationTestingReport otherReport;
            using (var streamReader = File.OpenText(fileName))
            {
                otherReport = (MutationTestingReport)new XmlSerializer(typeof(MutationTestingReport)).Deserialize(streamReader);
            }

            foreach (var sourceFile in otherReport.SourceFiles)
            {
                if (!SourceFiles.Any(s => s.Url == sourceFile.Url))
                {
                    SourceFiles.Add(sourceFile);
                }
                else
                {
                    SourceFiles.First(s => s.Url == sourceFile.Url).MergeFrom(sourceFile);
                }
            }
        }

        public void RegisterMethod(MethodDefinition method)
        {
            if (method.Body.Instructions.All(i => i.SequencePoint == null)) return;
            string sourceFileUrl =
                method.Body.Instructions.First(i => i.SequencePoint != null).SequencePoint.Document.Url;
            _readerWriterLock.EnterUpgradeableReadLock();
            try
            {
                if (!SourceFiles.Any(s => s.Url == sourceFileUrl))
                {
                    _readerWriterLock.EnterWriteLock();
                    var newSourceFile = new SourceFile();
                    newSourceFile.SetUrl(sourceFileUrl);
                    SourceFiles.Add(newSourceFile);
                    _readerWriterLock.ExitWriteLock();
                }
            }
            finally
            {
                _readerWriterLock.ExitUpgradeableReadLock();
            }
            var sourceFile = SourceFiles.First(s => s.Url == sourceFileUrl);
            var sequencePoints = method.Body.Instructions
                .Where(i => i.SequencePoint != null)
                .Select(i => i.SequencePoint).Distinct();
            foreach (var point in sequencePoints)
            {
                sourceFile.AddSequencePoint(point);
            }
        }

        public void AddResult(Mono.Cecil.Cil.SequencePoint sequencePoint, MutationTestMetaData mutationTestMetaData, bool mutantKilled)
        {
            if (sequencePoint == null || sequencePoint.Document == null) return;
            string sourceFileUrl = sequencePoint.Document.Url;
            _readerWriterLock.EnterUpgradeableReadLock();
            try
            {
                if (!SourceFiles.Any(s => s.Url == sourceFileUrl))
                {
                    _readerWriterLock.EnterWriteLock();
                    var newSourceFile = new SourceFile();
                    newSourceFile.SetUrl(sourceFileUrl);
                    SourceFiles.Add(newSourceFile);
                    _readerWriterLock.ExitWriteLock();
                }
            }
            finally
            {
                _readerWriterLock.ExitUpgradeableReadLock();
            }
            var sourceFile = SourceFiles.First(s => s.Url == sourceFileUrl);
            sourceFile.AddResult(sequencePoint, mutationTestMetaData, mutantKilled);
        }
    }
}
