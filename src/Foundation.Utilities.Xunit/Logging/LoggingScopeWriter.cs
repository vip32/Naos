namespace Naos.Foundation.Utilities.Xunit
{
    using System;
    using System.Globalization;
    using global::Xunit.Abstractions;
    using Naos.Foundation;
    using Newtonsoft.Json;

    public class LoggingScopeWriter : IDisposable
    {
        private readonly LoggingSettings settings;
        private readonly int depth;
        private readonly Action onScopeEnd;
        private readonly ITestOutputHelper outputHelper;
        private readonly object state;
        private string scopeMessage;
        private string structuredStateData;

        public LoggingScopeWriter(
            ITestOutputHelper outputHelper,
            object state,
            int depth,
            Action onScopeEnd,
            LoggingSettings settings)
        {
            this.outputHelper = outputHelper;
            this.state = state;
            this.depth = depth;
            this.onScopeEnd = onScopeEnd;
            this.settings = settings;

            this.DetermineScopeStateMessage();
            var scopeStartMessage = this.BuildScopeStateMessage(false);
            this.outputHelper.WriteLine(scopeStartMessage);

            if (!string.IsNullOrWhiteSpace(this.structuredStateData))
            {
                var padding = this.BuildPadding(this.depth + 1);
                var lines = this.structuredStateData.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                var data = padding + string.Join(Environment.NewLine + padding, lines);

                this.outputHelper.WriteLine("{0}{1}{2}{3}", padding, "Scope data: ", Environment.NewLine, data);
            }
        }

        public void Dispose()
        {
            var scopeStartMessage = this.BuildScopeStateMessage(true);
            this.outputHelper.WriteLine(scopeStartMessage);
            this.onScopeEnd?.Invoke();
        }

        private string BuildPadding(int depth)
        {
            return new string(' ', depth * this.settings.ScopePaddingSpaces);
        }

        private string BuildScopeStateMessage(bool isScopeEnd)
        {
            var padding = this.BuildPadding(this.depth);
            var endScopeMarker = isScopeEnd ? "/" : string.Empty;
            const string Format = "{0}<{1}{2}>";
            return string.Format(CultureInfo.InvariantCulture, Format, padding, endScopeMarker, this.scopeMessage);
        }

        private void DetermineScopeStateMessage()
        {
            const string ScopeMarker = "Scope: ";
            var defaultScopeMessage = "Scope " + (this.depth + 1);

            if (this.state == null)
            {
                this.scopeMessage = defaultScopeMessage;
            }
            else if (this.state is string state)
            {
                if (string.IsNullOrWhiteSpace(state))
                {
                    this.scopeMessage = defaultScopeMessage;
                }
                else
                {
                    this.scopeMessage = ScopeMarker + state;
                }
            }
            else if (this.state.GetType().IsValueType)
            {
                this.scopeMessage = ScopeMarker + this.state;
            }
            else
            {
                // The data is probably a complex object or a structured log entry
                this.structuredStateData = JsonConvert.SerializeObject(this.state, DefaultJsonSerializerSettings.Create());
                this.scopeMessage = defaultScopeMessage;
            }
        }
    }
}