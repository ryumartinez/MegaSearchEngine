using System.Diagnostics;

namespace Manager;

public static class InstrumentationHelper
{
    public static readonly ActivitySource Source = new("manager");
}