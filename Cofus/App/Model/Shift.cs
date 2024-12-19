using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Model;
public class Shift : INotifyPropertyChanged
{
    public DateTime ShiftDate { get; set; } // Ngày chấm công
    public bool MorningShift { get; set; } // Trạng thái ca sáng
    public bool AfternoonShift { get; set; } // Trạng thái ca chiều
    public string Note { get; set; } // Ghi chú (nếu có)
    public DateTime CreatedAt { get; set; } // Ngày tạo bản ghi
    public DateTime UpdatedAt { get; set; } // Ngày cập nhật bản ghi

    public event PropertyChangedEventHandler PropertyChanged;
}
