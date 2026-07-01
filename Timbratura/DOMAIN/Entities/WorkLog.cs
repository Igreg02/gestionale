using DevExpress.Xpo;
using System;

public class WorkLog : XPObject
{
    public WorkLog(Session session) : base(session) { }

    private string _description;
    public string Description
    {
        get => _description;
        set => SetPropertyValue(nameof(Description), ref _description, value);
    }

    private int _hoursCounter;
    public int HoursCounter
    {
        get => _hoursCounter;
        set => SetPropertyValue(nameof(HoursCounter), ref _hoursCounter, value);
    }
/*
    // Le relazioni si definiscono con l'attributo Association
    private Project _project;
    [Association("Project-WorkLogs")]
    public Project Project
    {
        get => _project;
        set => SetPropertyValue(nameof(Project), ref _project, value);
    }
    */
}