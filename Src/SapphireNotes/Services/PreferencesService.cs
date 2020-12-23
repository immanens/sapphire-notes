﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using SapphireNotes.Contracts.Models;

namespace SapphireNotes.Services
{
    public interface IPreferencesService
    {
        Preferences Preferences { get; }
        bool Load();
        void SavePreferences();
        void SaveWindowPreferences(int width, int height, int positionX, int positionY);
    }

    public class PreferencesService : IPreferencesService
    {
        private const string PreferencesFileName = "preferences.bin";
        private string _preferencesFilePath;

        public Preferences Preferences { get; private set; }

        public bool Load()
        {
            string appDataDirectory = string.Empty;

#if !DEBUG
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                appDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Globals.ApplicationName);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                appDataDirectory = Path.Combine("/var/lib", Globals.ApplicationName.ToLowerInvariant());
            }

            if (!Directory.Exists(appDataDirectory))
            {
                Directory.CreateDirectory(appDataDirectory);
            }
#endif

            _preferencesFilePath = Path.Combine(appDataDirectory, PreferencesFileName);

            if (File.Exists(_preferencesFilePath))
            {
                ReadPreferences();
                return Preferences.NotesDirectory != string.Empty;
            }

            Preferences = new Preferences();
            SavePreferences();

            return false;
        }

        public void SavePreferences()
        {
            using var writer = new BinaryWriter(File.Open(_preferencesFilePath, FileMode.OpenOrCreate));
            writer.Write(Preferences.NotesDirectory);
            writer.Write(Preferences.AutoSaveInterval);
            writer.Write(Preferences.Window.Width);
            writer.Write(Preferences.Window.Height);
            writer.Write(Preferences.Window.PositionX);
            writer.Write(Preferences.Window.PositionY);
        }

        public void SaveWindowPreferences(int width, int height, int positionX, int positionY)
        {
            if (width != Preferences.Window.Width || height != Preferences.Window.Height)
            {
                Preferences.Window.Width = width;
                Preferences.Window.Height = height;
            }

            Preferences.Window.PositionX = positionX;
            Preferences.Window.PositionY = positionY;

            SavePreferences();
        }

        private void ReadPreferences()
        {
            Preferences = new Preferences();

            using var reader = new BinaryReader(File.Open(_preferencesFilePath, FileMode.Open));
            Preferences.NotesDirectory = reader.ReadString();
            Preferences.AutoSaveInterval = reader.ReadInt16();
            Preferences.Window = new WindowPreferences
            {
                Width = reader.ReadInt32(),
                Height = reader.ReadInt32(),
                PositionX = reader.ReadInt32(),
                PositionY = reader.ReadInt32()
            };
        }
    }
}
