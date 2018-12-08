﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.SceneManagement;

public class DependencyViewer : EditorWindow
{
    public UnityEngine.Object refTarget;
    private DependencyViewerSettings _settings;
    internal DependencyViewerSettings Settings
    {
        get { return _settings; }
        set { _settings = value; }
    }

    private DependencyViewerGraph _graph;
    private DependencyViewerGraphDrawer _graphDrawer;
    private DependencyViewerSettingsOverlay _settingsOverlay;
    private DependencyResolver _resolver;

    private bool _readyToDrag;
    private bool _isDragging;


    [MenuItem("GameObject/View Dependencies", priority = 10)]
    private static void ViewReferenceInCurrentSceneFromMenuCommand(MenuCommand menuCommand)
    {
        ViewReferenceInCurrentScene();
    }

    [MenuItem("Assets/View Dependencies")]
    public static void ViewReferenceInCurrentScene()
    {
        DependencyViewer referenceViewer = EditorWindow.GetWindow<DependencyViewer>("Dependency Viewer");
        referenceViewer.Settings.ShouldSearchInCurrentScene = true;
        referenceViewer.ViewDependencies(Selection.activeObject);
    }

    public void ViewDependencies(UnityEngine.Object targetObject)
    {
        refTarget = targetObject;
        BuildGraph();
    }

    private void OnEnable()
    {
        _graph = new DependencyViewerGraph();
        _graphDrawer = new DependencyViewerGraphDrawer(_graph);
        _settings = DependencyViewerSettings.Create();
        _settingsOverlay = new DependencyViewerSettingsOverlay(_settings);
        _resolver = new DependencyResolver(_graph, _settings);

        _graphDrawer.requestViewDependency += ViewDependencies;
        _settingsOverlay.onSettingsChanged += OnSettingsChanged;

        if (refTarget != null)
        {
            BuildGraph();
        }
    }

    private void OnSettingsChanged()
    {
        BuildGraph();
    }

    public void BuildGraph()
    {
        _graph.CreateReferenceTargetNode(refTarget);
        _resolver.BuildGraph();
        _graph.RearrangeNodesLayout();
        CenterViewerOnGraph();
    }
    private void CenterViewerOnGraph()
    {
        _graphDrawer.CenterViewerOnGraph(position);
    }

    private void OnGUI()
    {
        if (refTarget == null)
        {
            return;
        }

        UpdateInputs();

        _graphDrawer.Draw();

        if (_settingsOverlay != null)
        {
            _settingsOverlay.Draw();
        }
    }

    private void UpdateInputs()
    {
        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 0 && position.Contains(e.mousePosition))
        {
            _readyToDrag = true;
        }

        if ((_readyToDrag || _isDragging) && e.type == EventType.MouseDrag && e.button == 0)
        {
            _readyToDrag = false;
            _isDragging = true;

            Vector2 offset = e.delta;
            _graphDrawer.ShiftView(-offset);

            Repaint();
        }

        if (_isDragging && e.type == EventType.MouseUp && e.button == 0)
        {
            _isDragging = false;
        }
    }
}
