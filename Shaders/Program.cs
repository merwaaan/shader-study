namespace Shaders
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var app = new App("Shader study", 900, 900))
                app.Run(60.0);
        }
    }
}
