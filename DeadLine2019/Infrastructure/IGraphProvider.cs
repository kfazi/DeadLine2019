namespace DeadLine2019.Infrastructure
{
    using Microsoft.Msagl.Drawing;

    public interface IGraphProvider
    {
        Graph Graph { get; set; }

        void ClearGraph();

        void UpdateLayout();

        void ShowGraph();

        void HideGraph();
    }
}