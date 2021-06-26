using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelEngine{
    public abstract class Initializer
    {
        public abstract void Initialize(Chunk chunk);
    }
}