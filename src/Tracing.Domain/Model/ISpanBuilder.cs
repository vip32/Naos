﻿namespace Naos.Core.Tracing.Domain
{
    public interface ISpanBuilder
    {
        ISpanBuilder IgnoreParentSpan();

        ISpanBuilder WithTag(string key, string value);

        ISpan Build();

        IScope Activate(bool finishOnDispose = true);
    }
}