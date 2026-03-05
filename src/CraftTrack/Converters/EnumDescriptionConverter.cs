using System.Globalization;
using System.Windows.Data;
using CraftTrack.Models.Enums;

namespace CraftTrack.Converters;

public class EnumDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Enum enumValue)
        {
            return enumValue switch
            {
                SeverityLevel.Rush => "Rush",
                SeverityLevel.High => "High",
                SeverityLevel.Low => "Low",
                OrderStatus.Active => "Active",
                OrderStatus.InProgress => "In Progress",
                OrderStatus.Completed => "Completed",
                OrderStatus.Cancelled => "Cancelled",
                PaymentType.FullPayment => "Full Payment",
                PaymentType.Downpayment => "Downpayment",
                PaymentType.Unpaid => "Unpaid",
                InventoryCategory.RawMaterial => "Raw Material",
                InventoryCategory.ToolAndEquipment => "Tool & Equipment",
                AdjustmentType.Add => "Add",
                AdjustmentType.Deduct => "Deduct",
                ExpenseCategory.Materials => "Materials",
                ExpenseCategory.Tools => "Tools",
                ExpenseCategory.Packaging => "Packaging",
                ExpenseCategory.Utilities => "Utilities",
                ExpenseCategory.Other => "Other",
                RejectReason.WrongPrinting => "Wrong Printing",
                RejectReason.IncorrectDesign => "Incorrect Design",
                RejectReason.MaterialDefect => "Material Defect",
                RejectReason.MachineMalfunction => "Machine Malfunction",
                RejectReason.CustomerReturn => "Customer Return",
                RejectReason.QualityControl => "Quality Control",
                RejectReason.Other => "Other (Reject)",
                NotificationType.RejectCreated => "Item Rejected",
                _ => enumValue.ToString()
            };
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
