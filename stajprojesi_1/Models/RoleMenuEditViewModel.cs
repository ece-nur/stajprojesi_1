namespace stajprojesi_1.Models
{
    public class RoleMenuEditViewModel
    {
        public int RoleId { get; set; }
        public List<Menu> AllMenus { get; set; }
        public List<int> SelectedMenuIds { get; set; }
    }

}
