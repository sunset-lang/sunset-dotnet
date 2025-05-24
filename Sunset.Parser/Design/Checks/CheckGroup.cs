namespace Sunset.Parser.Design;

// TODO: Not implemented yet

/*
/// <summary>
/// Contains a group of checks. Can check all checks in the group simultaneously.
/// </summary>
public class CheckGroup<T>(T element, string name) : ICheck where T : CheckableElementBase<T>
{
    public string Name { get; } = name;

    /// <inheritdoc />
    public bool? Pass { get; } = null;

    /// <summary>
    /// The element that the CheckGroup belongs to.
    /// </summary>
    public T Element { get; } = element;

    /// <summary>
    /// The checks within this check group
    /// </summary>
    public List<ICheck> Checks { get; } = [];
    

    /// <inheritdoc />
    public bool Check()
    {
        bool pass = true;
        foreach (ICheck check in Checks)
        {
            pass &= check.Check();
        }

        return pass;
    }

    public void AddCheck(ICheck check)
    {
        Checks.Add(check);
    }

    public ReportSection? DefaultReport { get; set; }
    
    public void AddToReport(ReportSection report)
    {
        throw new NotImplementedException();
    }

    public void AddToReport()
    {
        throw new NotImplementedException();
    }
}*/