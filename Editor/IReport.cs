namespace JellyTools.JellySceneResourcesReport
{
    public interface IReport : IDrawable
    {
        void Build();
        bool HasData { get; }
    }
}