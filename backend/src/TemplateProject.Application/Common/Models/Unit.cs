namespace TemplateProject.Application.Common.Models;

public sealed class Unit
{
    public static readonly Unit Value = new();
    private Unit() { }

    public override string ToString() => "()";
}
