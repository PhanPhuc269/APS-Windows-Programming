using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Model;
public class ShiftAttendance: INotifyPropertyChanged
{
    public int Id
    {
        get; set;
    } // ID của bản ghi
    public int EmployeeId
    {
        get; set;
    } // ID nhân viên
    public string Name { get; set; }
    public FullObservableCollection<Shift> Shifts
    {
        get; set;
    } // Danh sách ca làm việc


    public event PropertyChangedEventHandler PropertyChanged;

}
