namespace FormValidationTest.Client.Components.Forms;

public sealed record ChoiceOption<TValue>(TValue Value, string Label, string? HelpText = null);
