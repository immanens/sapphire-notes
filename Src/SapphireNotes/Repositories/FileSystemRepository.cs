using System;
using System.Collections.Generic;
using System.IO;
using SapphireNotes.Contracts;
using SapphireNotes.Contracts.Models;
using SapphireNotes.Exceptions;
using SapphireNotes.Services;
using SapphireNotes.Utils;

namespace SapphireNotes.Repositories;

public interface IFileSystemRepository : INotesRepository
{
    void MoveAll(string oldDirectory);
}

public class FileSystemRepository : IFileSystemRepository
{
    private const string Extension = ".txt";
    private readonly IPreferencesService _preferencesService;

    public FileSystemRepository(IPreferencesService preferencesService)
    {
        _preferencesService = preferencesService;
    }

    /// <summary>
    /// Создание заметки.
    /// </summary>
    /// <param name="name"> Имя заметки. </param>
    public void Create(string name)
    {
        var path = Path.Combine(_preferencesService.Preferences.NotesDirectory, name + Extension);
        File.Create(path).Dispose();
    }

    /// <summary>
    /// Создание заметки.
    /// </summary>
    /// <param name="name"> Имя заметки. </param>
    /// <param name="content"> Контент. </param>
    /// <returns></returns>
    public string Create(string name, string content)
    {
        var path = Path.Combine(_preferencesService.Preferences.NotesDirectory, name + Extension);
        path = FileUtil.NextAvailableFileName(path);

        using StreamWriter sw = File.CreateText(path);
        sw.Write(content);

        return Path.GetFileNameWithoutExtension(path);
    }

    /// <summary>
    /// Обновление заметки.
    /// </summary>
    /// <param name="name"> Текущее имя заметки. </param>
    /// <param name="newName"> Новое имя заметки.</param>
    public void Update(string name, string newName)
    {
        var path = Path.Combine(_preferencesService.Preferences.NotesDirectory, name + Extension);
        var newPath = Path.Combine(_preferencesService.Preferences.NotesDirectory, newName + Extension);
        File.Move(path, newPath);
    }

    /// <summary>
    /// Удаление заметки.
    /// </summary>
    /// <param name="name"> Имя заметки. </param>
    public void Delete(string name)
    {
        var path = Path.Combine(_preferencesService.Preferences.NotesDirectory, name + Extension);
        File.Delete(path);
    }

    /// <summary>
    /// Удаление архивной заметик.
    /// </summary>
    /// <param name="name"> Имя заметки. </param>
    public void DeleteArchived(string name)
    {
        var path = Path.Combine(GetArchiveDirectory(), name + Extension);
        File.Delete(path);
    }

    /// <summary>
    /// Сохранение заметки.
    /// </summary>
    /// <param name="name"> Имя заметки.</param>
    /// <param name="content"> Контент. </param>
    public void Save(string name, string content)
    {
        var path = Path.Combine(_preferencesService.Preferences.NotesDirectory, name + Extension);
        File.WriteAllText(path, content);
    }

    /// <summary>
    /// Поиск существующей заметик.
    /// </summary>
    /// <param name="name"> Имя искомой заметки. </param>
    /// <returns> Булево значение true - файл наден, false - не найден. </returns>
    public bool Exists(string name)
    {
        var path = Path.Combine(_preferencesService.Preferences.NotesDirectory, name + Extension);
        return File.Exists(path);
    }

    /// <summary>
    /// Архивная заметка.
    /// </summary>
    /// <param name="name"> Имя заметки. </param>
    /// <returns></returns>
    public string Archive(string name)
    {
        string archiveDirectory = GetArchiveDirectory();

        if (!Directory.Exists(archiveDirectory))
        {
            Directory.CreateDirectory(archiveDirectory);
        }

        var path = Path.Combine(_preferencesService.Preferences.NotesDirectory, name + Extension);
        var fileName = Path.GetFileName(path);

        var archivePath = Path.Combine(archiveDirectory, fileName);
        archivePath = FileUtil.NextAvailableFileName(archivePath);

        File.Move(path, archivePath);

        return Path.GetFileNameWithoutExtension(archivePath);
    }

    /// <summary>
    /// Восстановление заметки.
    /// </summary>
    /// <param name="name"> Имя заметки. </param>
    /// <returns> Имя восстановленной заметик. </returns>
    public string Restore(string name)
    {
        var archivePath = Path.Combine(GetArchiveDirectory(), name + Extension);
        var fileName = Path.GetFileName(archivePath);
        var path = Path.Combine(_preferencesService.Preferences.NotesDirectory, fileName);

        path = FileUtil.NextAvailableFileName(path);
        File.Move(archivePath, path);

        return Path.GetFileNameWithoutExtension(path);
    }

    /// <summary>
    /// Получение всех заметок.
    /// </summary>
    /// <returns> Коллекцию с заметками. </returns>
    public IEnumerable<Note> GetAll()
    {
        if (!Directory.Exists(_preferencesService.Preferences.NotesDirectory))
        {
            return Array.Empty<Note>();
        }

        string[] textFiles = Directory.GetFiles(_preferencesService.Preferences.NotesDirectory, "*" + Extension);
        var notes = new List<Note>(textFiles.Length);
        foreach (string filePath in textFiles)
        {
            string name = Path.GetFileNameWithoutExtension(filePath);
            string contents = File.ReadAllText(filePath);
            DateTime lastWriteTime = File.GetLastWriteTime(filePath);

            notes.Add(new Note(name, contents, lastWriteTime));
        }

        string archiveDirectory = GetArchiveDirectory();
        if (Directory.Exists(archiveDirectory))
        {
            string[] archivedTextFiles = Directory.GetFiles(archiveDirectory, "*" + Extension);
            foreach (string filePath in archivedTextFiles)
            {
                string name = Path.GetFileNameWithoutExtension(filePath);
                notes.Add(new Note(Globals.ArchivePrefix + "/" + name));
            }
        }

        return notes.ToArray();
    }

    /// <summary>
    /// Получение всех архивных заметок.
    /// </summary>
    /// <returns> Коллекцию с заметками. </returns>
    public IEnumerable<Note> GetAllArchived()
    {
        string archiveDirectory = GetArchiveDirectory();
        if (!Directory.Exists(archiveDirectory))
        {
            return Array.Empty<Note>();
        }

        string[] textFiles = Directory.GetFiles(archiveDirectory, "*" + Extension);
        if (textFiles.Length == 0)
        {
            return Array.Empty<Note>();
        }

        List<Note> notes = new List<Note>(textFiles.Length);
        foreach (string filePath in textFiles)
        {
            string name = Path.GetFileNameWithoutExtension(filePath);
            string contents = File.ReadAllText(filePath);

            notes.Add(new Note(name, contents));
        }

        return notes.ToArray();
    }

    /// <summary>
    /// Перемещение всех заметок.
    /// </summary>
    /// <param name="newDirectory"> Путь новой директории. </param>
    /// <exception cref="MoveNotesException"> Новая директория не найдена. </exception>
    public void MoveAll(string newDirectory)
    {
        string[] textFiles = Directory.GetFiles(_preferencesService.Preferences.NotesDirectory, "*" + Extension);

        var fromTo = new Dictionary<string, string>();
        foreach (string filePath in textFiles)
        {
            var newPath = Path.Combine(newDirectory, Path.GetFileName(filePath));
            if (!File.Exists(newPath))
            {
                fromTo.Add(filePath, newPath);
            }
            else
            {
                throw new MoveNotesException("Couldn't move the notes. " +
                                             "Make sure there aren't any existing notes with identical names in the chosen directory.");
            }
        }

        string archiveDirectory = GetArchiveDirectory();
        bool oldArchiveExists = Directory.Exists(archiveDirectory);
        if (oldArchiveExists)
        {
            string[] archivedTextFiles = Directory.GetFiles(archiveDirectory, "*" + Extension);
            if (archivedTextFiles.Length > 0)
            {
                string newArchivePath = Path.Combine(newDirectory, Globals.ArchivePrefix);
                if (!Directory.Exists(newArchivePath))
                {
                    Directory.CreateDirectory(newArchivePath);
                }

                foreach (var filePath in archivedTextFiles)
                {
                    var newPath = Path.Combine(newArchivePath, Path.GetFileName(filePath));
                    if (!File.Exists(newPath))
                    {
                        fromTo.Add(filePath, newPath);
                    }
                    else
                    {
                        throw new MoveNotesException("Couldn't move the archived notes. " +
                                                     "Make sure there aren't any existing notes with identical names in the chosen directory's " +
                                                     $"'{Globals.ArchivePrefix}' folder.");
                    }
                }
            }
        }

        foreach (var kvp in fromTo)
        {
            File.Move(kvp.Key, kvp.Value);
        }

        if (oldArchiveExists)
        {
            Directory.Delete(GetArchiveDirectory());
        }
    }

    /// <summary>
    /// Вспомогательный метод для нахождения активной директории.
    /// </summary>
    /// <returns> Активную директорию. </returns>
    private string GetArchiveDirectory()
    {
        return Path.Combine(_preferencesService.Preferences.NotesDirectory, Globals.ArchivePrefix);
    }
}
