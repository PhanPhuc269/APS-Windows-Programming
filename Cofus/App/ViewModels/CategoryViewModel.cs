using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Model;

namespace App.ViewModels;
public class CategoryViewModel
{
    public FullObservableCollection<Category> ListTypeBeverages
    {
        get; set;
    }
    public CategoryViewModel()
    {
        IDao dao = App.GetService<IDao>();//ServiceFactory.GetChildOf(typeof(IDao)) as IDao
        ListTypeBeverages = dao.GetListTypeBeverage();
    }
    public FullObservableCollection<Product> GetAllBeverage()
    {
        return App.GetService<IDao>().GetAllBeverage();
    }
}
