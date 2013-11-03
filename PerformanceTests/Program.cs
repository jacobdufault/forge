using LitJson;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Neon.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace PerformanceTests {
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

    public class MovementSystem1 : ITriggerUpdate, ITriggerAdded, ITriggerModified {
        public void OnAdded(IEntity entity) {
            Console.WriteLine("OnAdded " + entity + "; previous=" + entity.Previous<MyPositionData>() + "; current=" + entity.Current<MyPositionData>());
        }

        public void OnUpdate(IEntity entity) {
            Console.WriteLine("OnUpdate " + entity + "; previous=" + entity.Previous<MyPositionData>() + "; current=" + entity.Current<MyPositionData>());
            entity.Modify<MyPositionData>().X += 1;
        }

        public void OnModified(IEntity entity) {
            Console.WriteLine("OnModified " + entity + "; previous=" + entity.Previous<MyPositionData>() + "; current=" + entity.Current<MyPositionData>());
        }

        public Type[] ComputeEntityFilter() {
            return new[] { typeof(MyPositionData) };
        }

        public string RestorationGUID {
            get { return "0ec7763b6a614026a3fc080c7ecfbbf3"; }
        }

        public void Save(JsonData data) {
        }

        public void Restore(JsonData data) {
        }
    }

    public class MovementSystem2 : ITriggerUpdate, ITriggerModified {
        public void OnUpdate(IEntity entity) {
        }

        public void OnModified(IEntity entity) {
        }

        public Type[] ComputeEntityFilter() {
            return new[] { typeof(MyPositionData) };
        }

        public string RestorationGUID {
            get { return "c919bfe5d1b447bc8f45fdc087a5c38b"; }
        }

        public void Save(JsonData data) {
        }

        public void Restore(JsonData data) {
        }
    }

    public class SystemProvider : ISystemProvider {
        public ISystem[] GetSystems() {
            return new ISystem[] {
                new MovementSystem1(),
                new MovementSystem2()
            };
        }
    }

    class Program {
        private IEntity CreateEntity() {
            IEntity entity = new Entity();
            entity.AddData<MyPositionData>();
            return entity;
        }

        public Program() {

            EntityManager.EnableMultithreading = true;
            EntityManager entityManager = new EntityManager(new Entity());

            entityManager.AddSystem(new MovementSystem1());

            for (int i = 0; i < 50; ++i) {
                entityManager.AddSystem(new MovementSystem2());
            }


            for (int i = 0; i < 500; ++i) {
                entityManager.AddEntity(CreateEntity());
            }

            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            List<IStructuredInput> input = new List<IStructuredInput>();
            for (int i = 0; i < 1000; ++i) {
                entityManager.UpdateWorld(input).Wait();
            }
            //stopwatch.Stop();
            //Console.WriteLine(stopwatch.ElapsedMilliseconds);
            //Console.ReadLine();
        }

        static void Main(string[] args) {
            string fileText = File.ReadAllText("../../Level.json");
            //try {
            //LevelParser parser = new LevelParser();
            //parser.Parse(new StringReader(fileText));

            //Initialization init = JsonMapper.ToObject<Initialization>(fileText);
            //foreach (string dll in init.InjectedDlls) {
            //    Console.WriteLine("Found dll to load: " + dll);
            //}

            JsonReader reader = new JsonReader(fileText);
            reader.SkipNonMembers = false;
            reader.AllowComments = true;

            LevelJson level = JsonMapper.ToObject<LevelJson>(reader);
            EntityManager em = level.Restore();
            for (int i = 0; i < 3; ++i) {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
                em.UpdateWorld(new List<IStructuredInput>()).Wait();
            }

            //JsonData o = JsonMapper.ToObject(reader);
            //foreach (KeyValuePair<string, JsonData> entry in o) {
            //    Console.WriteLine(entry.Key + " => " + entry.Value);
            //}
            //Console.WriteLine(o);

            //}
            //catch (Exception e) {
            //    Console.WriteLine(e);
            //}
            Console.ReadLine();
            return;

            // file configuration failed; fall back to hard-coded backup; setup the default configuration
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
