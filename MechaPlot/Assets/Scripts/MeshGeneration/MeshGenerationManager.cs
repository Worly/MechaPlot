using System.Collections.Generic;

public static class MeshGenerationManager
{
    private static bool isPaused = false;
    private static HashSet<IMeshGenerator> queuedGenerators = new HashSet<IMeshGenerator>();

    public static void GenerateOrQueue(IMeshGenerator meshGenerator)
    {
        if (!isPaused)
            meshGenerator.GenerateMeshInternal();
        else
            queuedGenerators.Add(meshGenerator);
    }

    public static void Pause()
    {
        isPaused = true;
    }

    public static void UnPause()
    {
        isPaused = false;

        foreach (var generator in queuedGenerators)
            generator.GenerateMeshInternal();

        queuedGenerators.Clear();
    }
}
