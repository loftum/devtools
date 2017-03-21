namespace Pat.Models
{
    public class WindowSettings
    {
        public double Height { get; set; }
        public double Width { get; set; }

        public WindowSettings()
        {
            Height = 600;
            Width = 800;
        }
    }
}