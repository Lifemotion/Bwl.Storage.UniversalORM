﻿'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated by a tool.
'     Runtime Version:4.0.30319.42000
'
'     Changes to this file may cause incorrect behavior and will be lost if
'     the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict On
Option Explicit On

Imports System

Namespace My.Resources
    
    'This class was auto-generated by the StronglyTypedResourceBuilder
    'class via a tool like ResGen or Visual Studio.
    'To add or remove a member, edit your .ResX file then rerun ResGen
    'with the /str option, or rebuild your VS project.
    '''<summary>
    '''  A strongly-typed resource class, for looking up localized strings, etc.
    '''</summary>
    <Global.System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0"),  _
     Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
     Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute(),  _
     Global.Microsoft.VisualBasic.HideModuleNameAttribute()>  _
    Friend Module Resources
        
        Private resourceMan As Global.System.Resources.ResourceManager
        
        Private resourceCulture As Global.System.Globalization.CultureInfo
        
        '''<summary>
        '''  Returns the cached ResourceManager instance used by this class.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend ReadOnly Property ResourceManager() As Global.System.Resources.ResourceManager
            Get
                If Object.ReferenceEquals(resourceMan, Nothing) Then
                    Dim temp As Global.System.Resources.ResourceManager = New Global.System.Resources.ResourceManager("Bwl.Storage.UniversalORM.Resources", GetType(Resources).Assembly)
                    resourceMan = temp
                End If
                Return resourceMan
            End Get
        End Property
        
        '''<summary>
        '''  Overrides the current thread's CurrentUICulture property for all
        '''  resource lookups using this strongly typed resource class.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend Property Culture() As Global.System.Globalization.CultureInfo
            Get
                Return resourceCulture
            End Get
            Set
                resourceCulture = value
            End Set
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to SET ANSI_NULLS ON
        '''SET QUOTED_IDENTIFIER ON
        '''SET ANSI_PADDING ON
        '''CREATE TABLE [dbo].[{0}](
        '''	[guid] [nchar](38) NOT NULL,
        '''	[value] [bigint] NOT NULL,
        ''' CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED 
        '''(
        '''	[guid] ASC
        ''')WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
        ''') ON [PRIMARY]
        '''CREATE NONCLUSTERED INDEX [IX_{0}] ON [dbo].[{0}]
        '''(
        '''	[value] ASC
        ''')WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DRO [rest of string was truncated]&quot;;.
        '''</summary>
        Friend ReadOnly Property CreateBigIntIndexTableSQL() As String
            Get
                Return ResourceManager.GetString("CreateBigIntIndexTableSQL", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to SET ANSI_NULLS ON
        '''SET QUOTED_IDENTIFIER ON
        '''SET ANSI_PADDING ON
        '''CREATE TABLE [dbo].[{0}](
        '''	[guid] [nchar](38) NOT NULL,
        '''	[value] [float] NOT NULL,
        ''' CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED 
        '''(
        '''	[guid] ASC
        ''')WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
        ''') ON [PRIMARY]
        '''CREATE NONCLUSTERED INDEX [IX_{0}] ON [dbo].[{0}]
        '''(
        '''	[value] ASC
        ''')WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP [rest of string was truncated]&quot;;.
        '''</summary>
        Friend ReadOnly Property CreateFloatIndexTableSQL() As String
            Get
                Return ResourceManager.GetString("CreateFloatIndexTableSQL", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to SET ANSI_NULLS ON
        '''SET QUOTED_IDENTIFIER ON
        '''SET ANSI_PADDING ON
        '''CREATE TABLE [dbo].[{0}](
        '''	[guid] [nchar](38) NOT NULL,
        '''	[value] [int] NOT NULL,
        ''' CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED 
        '''(
        '''	[guid] ASC
        ''')WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
        ''') ON [PRIMARY]
        '''CREATE NONCLUSTERED INDEX [IX_{0}] ON [dbo].[{0}]
        '''(
        '''	[value] ASC
        ''')WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_E [rest of string was truncated]&quot;;.
        '''</summary>
        Friend ReadOnly Property CreateIntIndexTableSQL() As String
            Get
                Return ResourceManager.GetString("CreateIntIndexTableSQL", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to SET ANSI_NULLS ON
        '''SET QUOTED_IDENTIFIER ON
        '''SET ANSI_PADDING ON
        '''CREATE TABLE [dbo].[{0}](
        '''	[id] [int] IDENTITY(1,1) NOT NULL,
        '''	[guid] [nchar](38) NOT NULL,
        '''	[type] [nvarchar](max) NULL,
        '''	[json] [nvarchar](max) NULL,
        ''' CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED 
        '''(
        '''	[guid] ASC
        ''')WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
        ''') ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
        '''CREATE UNIQUE NONCLUSTERED INDEX [IX_{0}] ON [dbo].[{0 [rest of string was truncated]&quot;;.
        '''</summary>
        Friend ReadOnly Property CreateMainTableSQL() As String
            Get
                Return ResourceManager.GetString("CreateMainTableSQL", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to SET ANSI_NULLS ON
        '''SET QUOTED_IDENTIFIER ON
        '''SET ANSI_PADDING ON
        '''CREATE TABLE [dbo].[{0}](
        '''	[guid] [nchar](38) NOT NULL,
        '''	[value] [nvarchar]({2}) NOT NULL,
        ''' CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED 
        '''(
        '''	[guid] ASC
        ''')WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
        ''') ON [PRIMARY]
        '''CREATE NONCLUSTERED INDEX [IX_{0}] ON [dbo].[{0}]
        '''(
        '''	[value] ASC
        ''')WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = O [rest of string was truncated]&quot;;.
        '''</summary>
        Friend ReadOnly Property CreateStringIndexTableSQL() As String
            Get
                Return ResourceManager.GetString("CreateStringIndexTableSQL", resourceCulture)
            End Get
        End Property
    End Module
End Namespace
