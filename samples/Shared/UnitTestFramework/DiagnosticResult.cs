﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Roslyn.UnitTestFramework
{
    /// <summary>
    /// Structure that stores information about a <see cref="Diagnostic"/> appearing in a source.
    /// </summary>
    public struct DiagnosticResult
    {
        private const string DefaultPath = "Test0.cs";

        private static readonly object[] EmptyArguments = new object[0];

        private ImmutableArray<FileLinePositionSpan> _spans;
        private string _message;

        public DiagnosticResult(string id, DiagnosticSeverity severity)
            : this()
        {
            Id = id;
            Severity = severity;
        }

        public DiagnosticResult(DiagnosticDescriptor descriptor)
            : this()
        {
            Id = descriptor.Id;
            Severity = descriptor.DefaultSeverity;
            MessageFormat = descriptor.MessageFormat;
        }

        public ImmutableArray<FileLinePositionSpan> Spans
        {
            get
            {
                return _spans.IsDefault ? ImmutableArray<FileLinePositionSpan>.Empty : _spans;
            }
        }

        public DiagnosticSeverity Severity
        {
            get;
            private set;
        }

        public string Id
        {
            get;
            private set;
        }

        public string Message
        {
            get
            {
                if (_message != null)
                {
                    return _message;
                }

                if (MessageFormat != null)
                {
                    return string.Format(MessageFormat.ToString(), MessageArguments ?? EmptyArguments);
                }

                return null;
            }
        }

        public LocalizableString MessageFormat
        {
            get;
            private set;
        }

        public object[] MessageArguments
        {
            get;
            private set;
        }

        public bool HasLocation
        {
            get
            {
                return (_spans != null) && (_spans.Length > 0);
            }
        }

        public DiagnosticResult WithSeverity(DiagnosticSeverity severity)
        {
            DiagnosticResult result = this;
            result.Severity = severity;
            return result;
        }

        public DiagnosticResult WithArguments(params object[] arguments)
        {
            DiagnosticResult result = this;
            result.MessageArguments = arguments;
            return result;
        }

        public DiagnosticResult WithMessage(string message)
        {
            DiagnosticResult result = this;
            result._message = message;
            return result;
        }

        public DiagnosticResult WithMessageFormat(LocalizableString messageFormat)
        {
            DiagnosticResult result = this;
            result.MessageFormat = messageFormat;
            return result;
        }

        public DiagnosticResult WithLocation(int line, int column)
        {
            return WithLocation(DefaultPath, line, column);
        }

        public DiagnosticResult WithLocation(string path, int line, int column)
        {
            LinePosition linePosition = new LinePosition(line, column);

            return AppendSpan(new FileLinePositionSpan(path, linePosition, linePosition));
        }

        public DiagnosticResult WithSpan(int startLine, int startColumn, int endLine, int endColumn)
        {
            return WithSpan(DefaultPath, startLine, startColumn, endLine, endColumn);
        }

        public DiagnosticResult WithSpan(string path, int startLine, int startColumn, int endLine, int endColumn)
        {
            return AppendSpan(new FileLinePositionSpan(path, new LinePosition(startLine, startColumn), new LinePosition(endLine, endColumn)));
        }

        public DiagnosticResult WithLineOffset(int offset)
        {
            DiagnosticResult result = this;
            ImmutableArray<FileLinePositionSpan>.Builder spansBuilder = result._spans.ToBuilder();
            for (int i = 0; i < result._spans.Length; i++)
            {
                LinePosition newStartLinePosition = new LinePosition(result._spans[i].StartLinePosition.Line + offset, result._spans[i].StartLinePosition.Character);
                LinePosition newEndLinePosition = new LinePosition(result._spans[i].EndLinePosition.Line + offset, result._spans[i].EndLinePosition.Character);

                spansBuilder[i] = new FileLinePositionSpan(result._spans[i].Path, newStartLinePosition, newEndLinePosition);
            }

            result._spans = spansBuilder.MoveToImmutable();
            return result;
        }

        private DiagnosticResult AppendSpan(FileLinePositionSpan span)
        {
            ImmutableArray<FileLinePositionSpan> newSpans = _spans.Add(span);

            // clone the object, so that the fluent syntax will work on immutable objects.
            return new DiagnosticResult
            {
                Id = Id,
                _message = _message,
                MessageFormat = MessageFormat,
                MessageArguments = MessageArguments,
                Severity = Severity,
                _spans = newSpans,
            };
        }
    }
}
