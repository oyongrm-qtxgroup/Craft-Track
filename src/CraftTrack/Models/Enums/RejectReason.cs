using System.ComponentModel;

namespace CraftTrack.Models.Enums;

public enum RejectReason
{
    [Description("Wrong Printing")]
    WrongPrinting = 0,

    [Description("Incorrect Design")]
    IncorrectDesign = 1,

    [Description("Material Defect")]
    MaterialDefect = 2,

    [Description("Machine Malfunction")]
    MachineMalfunction = 3,

    [Description("Customer Return")]
    CustomerReturn = 4,

    [Description("Quality Control")]
    QualityControl = 5,

    [Description("Other")]
    Other = 6
}
