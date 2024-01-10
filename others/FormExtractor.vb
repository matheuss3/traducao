Imports System.IO
Imports System.Text
Imports System.Windows.Forms
Imports mtsMain

Public Class FormExtractor
    'Public Sub TranslateForm(_Me As Form, Optional lstTooltip As List(Of ToolTip) = Nothing)
    '    Try

    '        'Caso banco de dados nunca tenha sido carregado 
    '        'Nem perde tempo nessa parte
    '        If _scTraducao.lastSyncSQLite Is Nothing Then
    '            Exit Sub
    '        End If

    '        'Tradução do título da página
    '        _Me.Text = _Me.Text.Translated()

    '        Dim lstControls As New List(Of Control)
    '        Dim cLanguageTerms As New LanguageTerms
    '        GetControlsRecursively(_Me, lstControls)

    '        If (lstControls.Count() > 0) Then
    '            For index = 0 To lstControls.Count() - 1
    '                Dim controle = lstControls(index)
    '                Dim typeControl = controle.GetType()

    '                'Bloco para tratar controles específicos (ComboBox, DataGridView)
    '                Select Case typeControl
    '                    Case GetType(ComboBox)
    '                        Dim comboBox As ComboBox = DirectCast(controle, ComboBox)

    '                        If comboBox.Items.Count > 0 Then

    '                            'Só traduz se os itens da combobox forem strings simples
    '                            If TypeOf comboBox.Items(0) IsNot String Then
    '                                Continue For
    '                                'Exit Sub
    '                            End If

    '                            Dim listaOriginal As New List(Of Object)
    '                            For Each item In comboBox.Items
    '                                listaOriginal.Add(item)
    '                            Next

    '                            Dim selectedItem As Integer = comboBox.SelectedIndex

    '                            comboBox.Items.Clear()

    '                            For Each item As Object In listaOriginal
    '                                comboBox.Items.Add(cLanguageTerms.GetTermsTranslatedSQLite(item.ToString()))
    '                            Next

    '                            If comboBox.Items.Count > 0 Then
    '                                comboBox.SelectedIndex = selectedItem
    '                            End If
    '                        End If
    '                    Case GetType(DataGridView)
    '                        Dim dataGridView As DataGridView = DirectCast(controle, DataGridView)
    '                        TranslateGrid(dataGridView)
    '                    Case GetType(StatusStrip)
    '                        Dim ss As StatusStrip = DirectCast(controle, StatusStrip)
    '                        TranslateStripMenu(ss.Items)
    '                    Case GetType(MenuStrip)
    '                        Dim ms As MenuStrip = DirectCast(controle, MenuStrip)
    '                        TranslateStripMenu(ms.Items)
    '                    Case GetType(DateTimePicker)
    '                        Dim HasControls = typeControl.GetProperty("Text") IsNot Nothing

    '                        If HasControls AndAlso controle.Text <> "" Then
    '                            lstControls(index).Text = cLanguageTerms.GetTermsTranslatedSQLite(controle.Text)
    '                        End If
    '                        Dim currControl As DateTimePicker = lstControls(index)
    '                        currControl.UpdateMask

    '                    Case Else
    '                        Dim HasControls = typeControl.GetProperty("Text") IsNot Nothing

    '                        If HasControls AndAlso controle.Text <> "" Then
    '                            lstControls(index).Text = cLanguageTerms.GetTermsTranslatedSQLite(controle.Text)
    '                        End If
    '                End Select

    '                If controle.ContextMenuStrip IsNot Nothing Then
    '                    TranslateStripMenu(controle.ContextMenuStrip.Items)
    '                End If

    '                If lstTooltip IsNot Nothing Then
    '                    For Each tp As ToolTip In lstTooltip
    '                        tp.ToolTipTitle = tp.ToolTipTitle.Translated()

    '                        Dim toolTipControlText = tp.GetToolTip(controle)
    '                        If toolTipControlText <> "" Then
    '                            tp.SetToolTip(controle, toolTipControlText.Translated())
    '                        End If
    '                    Next
    '                End If
    '            Next
    '        End If
    '    Catch ex As Exception
    '        mtsMessage.Show(ex.Message, "TranslateForm", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '    End Try
    'End Sub
    Public Shared Sub ExtractFormInformation(form As Form, Optional lstToolTip As List(Of ToolTip) = Nothing)
        ' Local onde o csv com os termos serão salvos
        Dim PathRoot As String = "coloque_o_path_arqui"

        Using writer As New StreamWriter($"{PathRoot}\{form.Name}.csv")
            writer.WriteLine("Título da Tela;Nome do Elemento;Posição;Texto")
            ExtractControlInformation(writer, form.Text, form.Controls, lstToolTip)
        End Using
    End Sub

    Private Shared Sub SalvaMenuItensStrip(itens As ToolStripItemCollection, writer As StreamWriter,
                                    formTitle As String, location As String)
        For Each item As ToolStripItem In itens
            If item.Text <> "" Then

                WriteLine(writer, formTitle, item.Name, location, item.Text)
            End If
            ' Verificar se é uma opção de menu
            If TypeOf item Is ToolStripMenuItem Then


                ' Verificar se a opção possui subopções
                If DirectCast(item, ToolStripMenuItem).DropDownItems.Count > 0 Then
                    ' Se sim, chamar recursivamente a função para as subopções
                    SalvaMenuItensStrip(DirectCast(item, ToolStripMenuItem).DropDownItems, writer, formTitle, location)
                End If
            End If

        Next
    End Sub

    Private Shared Sub WriteLine(writer As StreamWriter, tituloTela As String, nomeElemento As String,
                          posicao As String, texto As String)

        If texto <> "" And texto = texto.Translated() Then
            writer.WriteLine($"{tituloTela};{nomeElemento};{posicao};{texto}")

        End If
    End Sub

    Private Shared Sub ExtractControlInformation(writer As StreamWriter, formTitle As String,
                                                 controls As Control.ControlCollection,
                                                 lstToolTip As List(Of ToolTip))
        For Each ctrl As Control In controls
            ' Se o controle tiver controles filhos, chama recursivamente a função
            If ctrl.Controls.Count > 0 Then
                ExtractControlInformation(writer, formTitle, ctrl.Controls, lstToolTip)
            End If

            ' Obtém o texto associado ao controle
            Dim controlText As String = ctrl.Text

            If TypeOf ctrl IsNot DateTimePicker Then
                ' Se o controle for uma ComboBox, inclua todos os itens
                If TypeOf ctrl Is ComboBox Then
                    Dim comboBox As ComboBox = DirectCast(ctrl, ComboBox)
                    For Each item As Object In comboBox.Items
                        ' Escreve uma linha para cada item
                        WriteLine(writer, formTitle, ctrl.Name, ctrl.Location.ToString, item.ToString())
                    Next
                ElseIf TypeOf ctrl Is Label Then
                    ' Se o controle for uma Label, inclua seu texto
                    WriteLine(writer, formTitle, ctrl.Name, ctrl.Location.ToString, controlText)
                    'ElseIf TypeOf ctrl Is DataGridView Then
                    '    ' Se o controle for uma DataGridView, inclua os nomes das colunas
                    '    Dim dataGridView As DataGridView = DirectCast(ctrl, DataGridView)
                    '    For Each column As DataGridViewColumn In dataGridView.Columns
                    '        writer.WriteLine($"{formTitle};{ctrl.Name};{ctrl.Location};{column.HeaderText}")
                    '    Next

                ElseIf TypeOf ctrl Is StatusStrip Then
                    Dim ss As StatusStrip = DirectCast(ctrl, StatusStrip)
                    SalvaMenuItensStrip(ss.Items, writer, formTitle, ss.Location.ToString)
                ElseIf TypeOf ctrl Is MenuStrip Then
                    Dim ms As MenuStrip = DirectCast(ctrl, MenuStrip)
                    SalvaMenuItensStrip(ms.Items, writer, formTitle, ms.Location.ToString)
                ElseIf TypeOf ctrl Is DataGridView Then
                    ' Se o controle for uma DataGridView, inclua os nomes das colunas
                    Dim dataGridView As DataGridView = DirectCast(ctrl, DataGridView)
                    For Each column As DataGridViewColumn In dataGridView.Columns
                        If column.ContextMenuStrip IsNot Nothing Then

                            SalvaMenuItensStrip(column.ContextMenuStrip.Items, writer, formTitle, ctrl.Location.ToString())
                        End If
                        If column.HeaderText.Trim <> "" Then

                            WriteLine(writer, formTitle, ctrl.Name, ctrl.Location.ToString, column.HeaderText)
                        End If
                    Next
                Else
                    WriteLine(writer, formTitle, ctrl.Name, ctrl.Location.ToString, controlText)
                End If
            End If

            If ctrl.ContextMenuStrip IsNot Nothing Then

                SalvaMenuItensStrip(ctrl.ContextMenuStrip.Items, writer, formTitle, ctrl.Location.ToString())
            End If
            If lstToolTip IsNot Nothing Then
                For Each tp As ToolTip In lstToolTip
                    WriteLine(writer, formTitle, $"tp_{tp.ToolTipTitle}__{ctrl.Name}", ctrl.Location.ToString, tp.GetToolTip(ctrl))
                Next
            End If



        Next
    End Sub
    Public Shared Sub SalvaGrid(dataGridView As DataGridView, writer As StreamWriter, formTitle As String)
        For Each column As DataGridViewColumn In dataGridView.Columns
            If column.HeaderText.Trim <> "" Then
                column.HeaderText = column.HeaderText.Translated()
                WriteLine(writer, $"{formTitle}__grid", dataGridView.Name, dataGridView.Location.ToString, column.HeaderText)
            End If
        Next
    End Sub

End Class