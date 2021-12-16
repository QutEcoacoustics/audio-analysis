// <copyright file="IModelWithMeta.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AcousticWorkbench.Models
{
    public interface IModelWithMeta
    {
        Meta Meta { get; set; }

        bool? Can(string action)
        {
            if (this.Meta?.Capabilities is { Count: > 0 })
            {
                if (this.Meta.Capabilities.ContainsKey(action))
                {
                    return this.Meta.Capabilities[action].Can;
                }
            }

            return null;
        }
    }
}