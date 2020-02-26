namespace Naos.Foundation.Utilities.Xunit
{
    using global::Xunit.Abstractions;
    using NetArchTest.Rules.Policies;

    public static class PolicyResultsReporter
    {
        /// <summary>
        /// Outputs a friendly display of the policy execution results;
        /// </summary>
        /// <param name="results"><see cref="PolicyResults"/> the policy results</param>
        /// <param name="output"><see cref="ITestOutputHelper"/> for outputs</param>
        public static void Write(PolicyResults results, ITestOutputHelper output)
        {
            if (results.HasViolations)
            {
                output.WriteLine($"Policy violations found for: {results.Name}");

                foreach (var rule in results.Results)
                {
                    if (!rule.IsSuccessful)
                    {
                        output.WriteLine("-----------------------------------------------------------");
                        output.WriteLine($"{rule.Name} - {rule.Description}");
                        foreach (var type in rule.FailingTypes)
                        {
                            output.WriteLine($"\t -> {type.FullName}");
                        }
                    }
                }

                output.WriteLine("-----------------------------------------------------------");
            }
            else
            {
                output.WriteLine($"No policy violations found for: {results.Name}");
            }
        }
    }
}
