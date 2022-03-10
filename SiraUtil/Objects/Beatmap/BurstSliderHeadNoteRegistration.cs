﻿using System;

namespace SiraUtil.Objects.Beatmap
{
    /// <summary>
    /// Registers a redecorator for the burst slider head (i.e. head in the chain).
    /// </summary>
    public sealed class BurstSliderHeadNoteRegistration : TemplateRedecoratorRegistration<GameNoteController, BeatmapObjectsInstaller>
    {
        /// <summary>
        /// Creates a new redecorator registration.
        /// </summary>
        /// <param name="redecorateCall">This is called when the object is being redecorated.</param>
        /// <param name="priority">The redecoration priority.</param>
        /// <param name="chain">Whether to chain this redecoration with others. Every redecoration is now aggregated.
        /// The chain will start if the highest priority object has chaining enabled and will stop once a registration
        /// in the aggregate has chaining disabled.</param>
        public BurstSliderHeadNoteRegistration(Func<GameNoteController, GameNoteController> redecorateCall, int priority = 0, bool chain = true) : base("_burstSliderHeadNotePrefab", redecorateCall, priority, chain) { }
    }
}