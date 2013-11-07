using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Neon.Entities;
using Neon.Entities.Serialization;
using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Neon.Performance.Tests {
    public class SpawnData : GameData<SpawnData> {
        public EntityTemplate SpawnedTemplate;

        public override void DoCopyFrom(SpawnData source) {
            this.SpawnedTemplate = source.SpawnedTemplate;
        }

        public override int HashCode {
            get { return 0; }
        }
    }

    public class MyPositionData : GameData<MyPositionData> {
        public int X;
        public int Z;

        public override string ToString() {
            return "MyPositionData [X=" + X + " Z=" + Z + "]";
        }

        public override void DoCopyFrom(MyPositionData source) {
            this.X = source.X;
            this.Z = source.Z;
        }

        public override int HashCode {
            get { return 0; }
        }
    }

    public class MyLocomotionData : GameData<MyLocomotionData> {
        public int Speed;
        public int Variance;

        public override string ToString() {
            return "MyLocomotionData Speed=" + Speed + " Variance=" + Variance;
        }

        public override void DoCopyFrom(MyLocomotionData source) {
            this.Speed = source.Speed;
            this.Variance = source.Variance;
        }

        public override int HashCode {
            get { return 0; }
        }
    }

    public class MovementSystem : ITriggerUpdate, ITriggerAdded, ITriggerModified {
        public void OnAdded(IEntity entity) {
            //Console.WriteLine("OnAdded " + entity + "; previous=" + entity.Previous<MyPositionData>() + "; current=" + entity.Current<MyPositionData>());
        }

        public void OnUpdate(IEntity entity) {
            //Console.WriteLine("OnUpdate " + entity + "; previous=" + entity.Previous<MyPositionData>() + "; current=" + entity.Current<MyPositionData>());
            entity.Modify<MyPositionData>().X += 1;
        }

        public void OnModified(IEntity entity) {
            //Console.WriteLine("OnModified " + entity + "; previous=" + entity.Previous<MyPositionData>() + "; current=" + entity.Current<MyPositionData>());
        }

        public Type[] ComputeEntityFilter() {
            return new[] { typeof(MyPositionData) };
        }
    }

    public class SpawnSystem : ITriggerUpdate {
        public void OnUpdate(IEntity entity) {
            //Console.WriteLine("Running SpawnSystem.OnUpdate for " + entity);
            entity.Current<SpawnData>().SpawnedTemplate.Instantiate();
        }

        public Type[] ComputeEntityFilter() {
            return new[] { typeof(SpawnData) };
        }
    }

    public class SystemProvider : ISystemProvider {
        public ISystem[] GetSystems() {
            return new ISystem[] {
                new MovementSystem(),
                new SpawnSystem()
            };
        }
    }

    internal class Program {
        private IEntity CreateEntity() {
            IEntity entity = new Entity();
            entity.AddData<MyPositionData>();
            return entity;
        }

        private static void Main(string[] args) {
            Console.WriteLine("...loading");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Tuple<EntityManager, LoadedMetadata> loadedLevel = Loader.LoadEntityManager("../../Level.nes");
            stopwatch.Stop();
            Console.WriteLine("Done; loading the level took " + stopwatch.ElapsedTicks + " ticks (or " + stopwatch.ElapsedMilliseconds + "ms)");
            EntityManager entityManager = loadedLevel.Item1;

            stopwatch.Reset();
            stopwatch.Start();
            for (int i = 0; i < 1; ++i) {
                entityManager.UpdateWorld(new List<IStructuredInput>()).Wait();
            }
            stopwatch.Stop();
            Console.WriteLine("Done; updating took " + stopwatch.ElapsedTicks + " ticks (or " + stopwatch.ElapsedMilliseconds + "ms)");

            stopwatch.Reset();
            stopwatch.Start();
            string saved = Loader.SaveEntityManager(loadedLevel.Item1, loadedLevel.Item2);
            stopwatch.Stop();
            Console.WriteLine("Done; saving took " + stopwatch.ElapsedTicks + " ticks (or " + stopwatch.ElapsedMilliseconds + "ms)");

            File.WriteAllText("../../SavedLevel.nes", saved);

            Console.WriteLine();
            Console.WriteLine("Hit enter to exit");
            Console.ReadLine();
            return;

            // file configuration failed; fall back to hard-coded backup; setup the default
            // configuration
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Shutdown();
            hierarchy.ResetConfiguration();
            hierarchy.Root.Level = log4net.Core.Level.All;

            ConfigureAppenders();

            new Program();

        }

        private static void ConfigureAppenders() {
            PatternLayout layout = new PatternLayout();
            layout.ConversionPattern = "%date [%thread] %-5level %logger - %message%n";
            layout.ActivateOptions();

            ConsoleAppender consoleAppener = new ConsoleAppender();
            consoleAppener.Layout = layout;
            consoleAppener.Threshold = log4net.Core.Level.Debug;
            consoleAppener.Target = ConsoleAppender.ConsoleOut;
            consoleAppener.ActivateOptions();

            PatternLayout fileLayout = new PatternLayout();
            fileLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message%n";
            fileLayout.ActivateOptions();

            FileAppender fileAppender = new FileAppender();
            fileAppender.AppendToFile = true;
            fileAppender.Threshold = log4net.Core.Level.Debug;
            fileAppender.File = string.Format("Logs/log_{0:MM-dd-yyyy_HH-mm-ss}.txt", DateTime.Now);
            fileAppender.Layout = fileLayout;
            fileAppender.ImmediateFlush = false;
            fileAppender.ActivateOptions();

            BasicConfigurator.Configure(fileAppender, fileAppender);
        }
    }
}