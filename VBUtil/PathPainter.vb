Namespace Utils

    '我不知道为什么画个图要弄得如此复杂
    Public Class PathPainter
        Private _useAA As Boolean '是否使用抗锯齿
        Private _paintXAxis, _paintYAxis As Boolean '是否绘制坐标轴

        Private _autosizeXAxis, _autosizeYAxis As Boolean '是否自动调整坐标轴的显示范围
        Private _constCentralZero As Boolean '坐标轴焦点是否强制为零点
        Private _minXAxis, _maxXAxis, _minYAxis, _maxYAxis As Double '坐标轴最值，在自动调整下，该值会由程序自动修改
        Private _XAxisColor, _YAxisColor As Color '坐标轴颜色

        Private _paintXMark, _paintYMark As Boolean '是否绘制坐标轴刻度
        Private _autosizeXMark, _autosizeYMark As Boolean '是否自动调整坐标轴刻度
        Private _markXAxis, _markYAxis As Double '坐标刻度间隔的值，在自动调整下，该值会由程序自动修改
#Region "Properties"
        ''' <summary>
        ''' 图片是否使用抗锯齿效果
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property UseAA As Boolean
            Get
                Return _useAA
            End Get
            Set(value As Boolean)
                _useAA = value
                _img_need_update = True
            End Set
        End Property
        ''' <summary>
        ''' 是否绘制X轴
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property PaintXAxis As Boolean
            Get
                Return _paintXAxis
            End Get
            Set(value As Boolean)
                _paintXAxis = value
                _img_need_update = True
            End Set
        End Property
        ''' <summary>
        ''' 是否绘制Y轴
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property PaintYAxis As Boolean
            Get
                Return _paintYAxis
            End Get
            Set(value As Boolean)
                _paintYAxis = value
                _img_need_update = True
            End Set
        End Property
        ''' <summary>
        ''' 是否自动调整X轴的显示范围
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property AutosizeXAxis As Boolean
            Get
                Return _autosizeXAxis
            End Get
            Set(value As Boolean)
                _autosizeXAxis = value
                _img_need_update = True
                _x_mark_need_update = True
            End Set
        End Property
        ''' <summary>
        ''' 是否自动调整Y轴的显示范围
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property AutosizeYAxis As Boolean
            Get
                Return _autosizeYAxis
            End Get
            Set(value As Boolean)
                _autosizeYAxis = value
                _img_need_update = True
                _y_mark_need_update = True
            End Set
        End Property
        ''' <summary>
        ''' 是否强制为坐标原点为零点
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ConstCentralZero As Boolean
            Get
                Return _constCentralZero
            End Get
            Set(value As Boolean)
                _constCentralZero = value

                _img_need_update = True
                _x_mark_need_update = True
                _y_mark_need_update = True

                _calculate_axis_value()
            End Set
        End Property
        ''' <summary>
        ''' X轴坐标的最小值（注意，如果启用自动调整刻度或坐标原点为零点后，该值可能会被程序自行修改）
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property MinXAxisValue As Double
            Get
                Return _minXAxis
            End Get
            Set(value As Double)
                _autosizeXAxis = False
                If value <= _maxXAxis Then
                    _minXAxis = value
                Else
                    _minXAxis = _maxXAxis
                    _maxXAxis = value
                End If
                _img_need_update = True
                _x_mark_need_update = True

                _calculate_axis_value()
            End Set
        End Property
        ''' <summary>
        ''' X轴坐标的最大值（注意，如果启用自动调整刻度或坐标原点为零点后，该值可能会被程序自行修改）
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property MaxXAxisValue As Double
            Get
                Return _maxXAxis
            End Get
            Set(value As Double)
                _autosizeXAxis = False
                If value >= _minXAxis Then
                    _maxXAxis = value
                Else
                    _maxXAxis = _minXAxis
                    _minXAxis = value
                End If
                _img_need_update = True
                _x_mark_need_update = True

                _calculate_axis_value()
            End Set
        End Property
        ''' <summary>
        ''' Y轴坐标的最小值（注意，如果启用自动调整刻度或坐标原点为零点后，该值可能会被程序自行修改）
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property MinYAxisValue As Double
            Get
                Return _minYAxis
            End Get
            Set(value As Double)
                _autosizeYAxis = False
                If value <= _maxYAxis Then
                    _minYAxis = value
                Else
                    _minYAxis = _maxYAxis
                    _maxYAxis = value
                End If
                _img_need_update = True
                _y_mark_need_update = True

                _calculate_axis_value()
            End Set
        End Property
        ''' <summary>
        ''' Y轴坐标的最大值（注意，如果启用自动调整刻度或坐标原点为零点后，该值可能会被程序自行修改）
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property MaxYAxisValue As Double
            Get
                Return _maxYAxis
            End Get
            Set(value As Double)
                _autosizeYAxis = False
                If value >= _minYAxis Then
                    _maxYAxis = value
                Else
                    _maxYAxis = _minYAxis
                    _minYAxis = value
                End If
                _img_need_update = True
                _y_mark_need_update = True

                _calculate_axis_value()
            End Set
        End Property
        ''' <summary>
        ''' X轴坐标线的颜色
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property XAxisColor As Color
            Get
                Return _XAxisColor
            End Get
            Set(value As Color)
                _XAxisColor = value
                _img_need_update = True
            End Set
        End Property
        ''' <summary>
        ''' Y轴坐标线的颜色
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property YAxisColor As Color
            Get
                Return _YAxisColor
            End Get
            Set(value As Color)
                _YAxisColor = value
                _img_need_update = True
            End Set
        End Property
        ''' <summary>
        ''' 是否绘制X坐标刻度线
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property PaintXMark As Boolean
            Get
                Return _paintXMark
            End Get
            Set(value As Boolean)
                _paintXMark = value
                _img_need_update = True
            End Set
        End Property
        ''' <summary>
        ''' 是否绘制Y坐标刻度线
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property PaintYMark As Boolean
            Get
                Return _paintYMark
            End Get
            Set(value As Boolean)
                _paintYMark = value
                _img_need_update = True
            End Set
        End Property
        ''' <summary>
        ''' 是否自动调整X轴坐标刻度线
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property AutosizeXMark As Boolean
            Get
                Return _autosizeXMark
            End Get
            Set(value As Boolean)
                _autosizeXMark = value
                _img_need_update = True
                _x_mark_need_update = True
            End Set
        End Property
        ''' <summary>
        ''' 是否自动调整Y轴坐标刻度线
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property AutosizeYMark As Boolean
            Get
                Return _autosizeYMark
            End Get
            Set(value As Boolean)
                _autosizeYMark = value
                _img_need_update = True
                _y_mark_need_update = True
            End Set
        End Property
        ''' <summary>
        ''' X轴坐标刻度线的间隔数值
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property XAxisMark As Double
            Get
                Return _markXAxis
            End Get
            Set(value As Double)
                _markXAxis = If(value > 0, value, 0)
                _img_need_update = True
                _x_mark_need_update = True
            End Set
        End Property
        ''' <summary>
        ''' Y轴坐标刻度线的间隔数值
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property YAxisMark As Double
            Get
                Return _markYAxis
            End Get
            Set(value As Double)
                _markYAxis = If(value > 0, value, 0)
                _img_need_update = True
                _y_mark_need_update = True
            End Set
        End Property
        ''' <summary>
        ''' 坐标原点在图中的位置
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property AxisCenter As Point
            Get
                Return New Point(_AxisCenter.X + _margin.Left, _AxisCenter.Y + _margin.Top)
            End Get
            Set(value As Point)
                _AxisCenter = New Point(value.X - _margin.Left, value.Y - _margin.Top)
                If _AxisCenter.X < 0 Then
                    _AxisCenter.X = 0
                ElseIf _AxisCenter.X > _margin.ContainerWidth Then
                    _AxisCenter.X = _margin.ContainerWidth
                End If

                If _AxisCenter.Y < 0 Then
                    _AxisCenter.Y = 0
                ElseIf _AxisCenter.Y > _margin.ContainerHeight Then
                    _AxisCenter.Y = _margin.ContainerHeight
                End If

                _img_need_update = True
                _x_mark_need_update = True
                _y_mark_need_update = True

                _calculate_axis_value()
            End Set
        End Property
        ''' <summary>
        ''' 是否绘制X轴坐标刻度的值
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property PaintXAxisValue As Boolean
            Get
                Return _paintXAxisValue
            End Get
            Set(value As Boolean)
                _paintXAxisValue = value
                _img_need_update = True
            End Set
        End Property
        ''' <summary>
        ''' 是否绘制Y轴坐标刻度的值
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property PaintYAxisValue As Boolean
            Get
                Return _paintYAxisValue
            End Get
            Set(value As Boolean)
                _paintYAxisValue = value
                _img_need_update = True
            End Set
        End Property
        ''' <summary>
        ''' 获取或设置曲线的颜色
        ''' </summary>
        ''' <param name="index"></param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property PathColor(ByVal index As Integer) As Color
            Get
                If index >= _path_color.Count Then Return Color.FromArgb(0)
                Return _path_color(index)
            End Get
            Set(value As Color)
                If index >= _path_color.Count Then Return
                _path_color(index) = value
                _img_need_update = True
            End Set
        End Property
        ''' <summary>
        ''' 获取或设置曲线路径点
        ''' </summary>
        ''' <param name="index"></param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Paths(ByVal index As Integer) As List(Of PointD)
            Get
                If index >= _paths.Count Then Return New List(Of PointD)
                Return _paths(index)
            End Get
            Set(value As List(Of PointD))
                If index >= _paths.Count Then Return
                SetPath(index, value)
                _img_need_update = True
            End Set
        End Property
        ''' <summary>
        ''' 输出图片的宽度
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Width As UInteger
            Get
                Return _width
            End Get
            Set(value As UInteger)
                _width = value
                _margin.Width = value
                _img_need_update = True
                _x_mark_need_update = True
                _y_mark_need_update = True
            End Set
        End Property
        ''' <summary>
        ''' 输出图片的高度
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Height As UInteger
            Get
                Return _height
            End Get
            Set(value As UInteger)
                _height = value
                _margin.Height = Height
                _img_need_update = True
                _x_mark_need_update = True
                _y_mark_need_update = True
            End Set
        End Property
        ''' <summary>
        ''' 最小标记的距离
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property MinXMarkDistance As UInteger
            Get
                Return _minXMarkDistance
            End Get
            Set(value As UInteger)
                _minXMarkDistance = value
            End Set
        End Property
        Public Property MinYMarkDistance As UInteger
            Get
                Return _minYMarkDistance
            End Get
            Set(value As UInteger)
                _minYMarkDistance = value
            End Set
        End Property
        Public ReadOnly Property MaxValue() As PointD
            Get
                Return _maxValue
            End Get
        End Property
        Public ReadOnly Property MinValue() As PointD
            Get
                Return _minValue
            End Get
        End Property
#End Region
        Public Structure PointD
            Public X, Y As Double
            Sub New(ByVal _x As Double, ByVal _y As Double)
                X = _x
                Y = _y
            End Sub
            Sub New(ByVal _p As Point)
                X = _p.X
                Y = _p.Y
            End Sub
            Sub New(ByVal _p As PointF)
                X = _p.X
                Y = _p.Y
            End Sub
            Sub New(ByVal _p As PointD)
                X = _p.X
                Y = _p.Y
            End Sub

            Public Overrides Function ToString() As String
                Return "X = " & X & " Y = " & Y
            End Function
        End Structure
        Private _AxisCenter As Point '坐标原点在图中的位置
        Private _paintXAxisValue, _paintYAxisValue As Boolean '是否绘制坐标刻度的值
        'Public Delegate Function ExternalGetAxisValue(ByVal is_XAxis As Boolean, ByVal value As Double) As String '重写坐标刻度值获取函数
        Public Structure ExternalGetAxisValueEventArg
            Public is_XAxis As Boolean
            Public value As Double
            Public display_str As String
        End Structure
        Public Event ExternalGetAxisValue(ByRef e As ExternalGetAxisValueEventArg) '重写坐标刻度值获取函数 -> 什么都不会还是老老实实事件回调吧
        Public Delegate Sub ExternalGetAxisValueHandler(ByRef e As ExternalGetAxisValueEventArg)
        Private Function _defaultGetAxisValue(ByVal is_XAxis As Boolean, ByVal value As Double) As String
            Return value
        End Function

        Private _path_color As List(Of Color)
        Private _paths As List(Of List(Of PointD))

        Private _width, _height As UInteger
        Public Structure Margin
            Private _left, _top, _right, _bottom As UInteger
            Public Property Left As UInteger
                Get
                    Return _left
                End Get
                Set(value As UInteger)
                    _left = If(value > _width, _width, value)
                End Set
            End Property
            Public Property Top As UInteger
                Get
                    Return _top
                End Get
                Set(value As UInteger)
                    _top = If(value > _top, _top, value)
                End Set
            End Property
            Public Property Right As UInteger
                Get
                    Return _right
                End Get
                Set(value As UInteger)
                    _right = If(_left + value > _width, _width - _left, value)
                End Set
            End Property
            Public Property Bottom As UInteger
                Get
                    Return _bottom
                End Get
                Set(value As UInteger)
                    _bottom = If(_top + _bottom > _height, _height - _top, value)
                End Set
            End Property
            Public Property Width As UInteger
                Get
                    Return _width
                End Get
                Set(value As UInteger)
                    If _left > value Then
                        _left = value
                        _right = 0
                    ElseIf _left + _right > value Then
                        _right = _width - _left
                    End If
                    _width = value
                End Set
            End Property
            Public Property Height As UInteger
                Get
                    Return _height
                End Get
                Set(value As UInteger)
                    If _top > value Then
                        _top = value
                        _bottom = 0
                    ElseIf _top + _bottom > value Then
                        _bottom = _height - _top
                    End If
                    _height = value
                End Set
            End Property
            Private _width, _height As UInteger
            Public ReadOnly Property ContainerWidth As UInteger
                Get
                    Return _width - _left - _right
                End Get
            End Property
            Public ReadOnly Property ContainerLeft As UInteger
                Get
                    Return _left
                End Get
            End Property
            Public ReadOnly Property ContainerRight As UInteger
                Get
                    Return _width - _right
                End Get
            End Property
            Public ReadOnly Property ContainerHeight As UInteger
                Get
                    Return _height - _top - _bottom
                End Get
            End Property
            Public ReadOnly Property ContainerTop As UInteger
                Get
                    Return _top
                End Get
            End Property
            Public ReadOnly Property ContainerBottom As UInteger
                Get
                    Return _height - _bottom
                End Get
            End Property
            Public ReadOnly Property ContainerCenter As PointD
                Get
                    Return New PointD((ContainerLeft + ContainerRight) / 2, (ContainerTop + ContainerBottom) / 2)
                End Get
            End Property
            Sub New(ByVal __width As UInteger, ByVal __height As UInteger, ByVal __top As UInteger, ByVal __bottom As UInteger, ByVal __left As UInteger, ByVal __right As UInteger)
                _width = __width
                _height = __height
                _left = __left
                _right = __right
                _top = __top
                _bottom = __bottom
            End Sub

            Public Overrides Function ToString() As String
                Return _width & "x" & _height & " [Margin : {L = " & _left & ", R = " & _right & ", T = " & _top & ", B = " & _bottom & "}, Container: " & ContainerWidth & "x" & ContainerHeight & "]"
            End Function
        End Structure

        Private _margin As Margin
        Private _minValue, _maxValue As PointD
        Private _x_mark_need_update, _y_mark_need_update As Boolean

        Private _img_cache As Image '不包含margin的图片
        Private _img_need_update As Boolean
        Private _gr As Graphics

        Private _out_img_cache As Image

        Private _minXMarkDistance As UInteger
        Private _minYMarkDistance As UInteger
        Public Sub New(ByVal width As UInteger, ByVal height As UInteger)
            _path_color = New List(Of Color)
            _paths = New List(Of List(Of PointD))

            _width = width
            _height = height

            'default setting
            _useAA = True
            _paintXAxis = True
            _paintYAxis = True
            _autosizeXAxis = True
            _autosizeYAxis = True
            _autosizeXMark = True
            _autosizeYMark = True
            _paintXAxisValue = True
            _paintYAxisValue = True
            _paintXMark = True
            _paintYMark = True
            _constCentralZero = False

            _XAxisColor = Color.Black
            _YAxisColor = Color.Black

            _margin = New Margin(_width, _height, 10, 10, 10, 10)
            _AxisCenter = New Point(_margin.ContainerCenter.X - _margin.Left, _margin.ContainerCenter.Y - _margin.Top)

            _minXMarkDistance = 30
            _minYMarkDistance = 30

            _img_need_update = True
            _x_mark_need_update = True
            _y_mark_need_update = True
        End Sub

        Public Function GetImage() As Image
            If _img_need_update Then _repaint()

            _img_need_update = False
            Return _out_img_cache
        End Function
        Private _value_init As Boolean = False
        Private Sub _update_value(ByVal pt As PointD)
            If Not _value_init Then
                _value_init = True
                _maxValue.X = pt.X
                _minValue.X = pt.X
                _maxValue.Y = pt.Y
                _minValue.Y = pt.Y
                _x_mark_need_update = True
                _y_mark_need_update = True
            End If
            If pt.X > _maxValue.X Then _maxValue.X = pt.X : _x_mark_need_update = True
            If pt.X < _minValue.X Then _minValue.X = pt.X : _x_mark_need_update = True
            If pt.Y > _maxValue.Y Then _maxValue.Y = pt.Y : _y_mark_need_update = True
            If pt.Y < _minValue.Y Then _minValue.Y = pt.Y : _y_mark_need_update = True
        End Sub
        Public Sub AddPath(ByVal ePath As IEnumerable(Of Point), ByVal color As Color)
            Dim new_ls As New List(Of PointD)
            For Each e As Point In ePath
                Dim pt As New PointD(e.X, e.Y)
                new_ls.Add(pt)
                _update_value(pt)
            Next
            _paths.Add(new_ls)
            _path_color.Add(color)
        End Sub
        Public Sub AddPath(ByVal ePath As IEnumerable(Of PointF), ByVal color As Color)
            Dim new_ls As New List(Of PointD)
            For Each e As PointF In ePath
                Dim pt As New PointD(e.X, e.Y)
                new_ls.Add(pt)
                _update_value(pt)
            Next
            _paths.Add(new_ls)
            _path_color.Add(color)
        End Sub
        Public Sub AddPath(ByVal ePath As IEnumerable(Of PointD), ByVal color As Color)
            Dim new_ls As New List(Of PointD)
            For Each e As PointD In ePath
                new_ls.Add(e)
                _update_value(e)
            Next
            _paths.Add(new_ls)
            _path_color.Add(color)
        End Sub
        Public Sub DeletePath(ByVal index As Integer)
            If index >= _paths.Count Then Return
            _path_color.RemoveAt(index)
            _paths.RemoveAt(index)
            _recalculate_axis_value()
        End Sub
        Private Sub _recalculate_axis_value()
            _value_init = False
            For Each e As List(Of PointD) In _paths
                For Each ee As PointD In e
                    _update_value(ee)
                Next
            Next
        End Sub
        Public Function GetPath(ByVal index As Integer) As List(Of PointD)
            If index >= _paths.Count Then Return New List(Of PointD)
            Return _paths(index)
        End Function

        Public Sub SetPath(ByVal index As Integer, ByVal ePath As IEnumerable(Of PointD))
            If index >= _paths.Count Then Return
            Dim ls As New List(Of PointD)
            For Each e As PointD In ePath
                ls.Add(e)
                _update_value(e)
            Next
            _paths(index) = ls
            _recalculate_axis_value()
        End Sub

        Public Sub ClearPath()
            _paths.Clear()
            _path_color.Clear()
            _recalculate_axis_value()
        End Sub
        Public Function GetPathColor(ByVal index As Integer) As Color
            If index >= _paths.Count Then Return Color.FromArgb(0)
            Return _path_color(index)
        End Function

        Public Sub SetPathColor(ByVal index As Integer, ByVal color As Color)
            If index >= _paths.Count Then Return
            _path_color(index) = color
        End Sub

        Private Sub _repaint()
            ' here to init graphics setting
            _init_graphics()

            _calculate_axis_value()

            _calculate_mark()
            _repaint_mark()

            _repaint_axis()

            _repaint_paths()

            _out_img_cache = New Bitmap(CInt(_width), CInt(_height))
            Dim _gr1 As Graphics = Graphics.FromImage(_out_img_cache)
            _gr1.DrawImage(_img_cache, _margin.Left, _margin.Top, _margin.ContainerWidth, _margin.ContainerHeight)

            _gr1.Dispose()

        End Sub

        '初始化绘制
        Private Sub _init_graphics()
            _img_cache = New Bitmap(CInt(_margin.ContainerWidth), CInt(_margin.ContainerHeight))
            _gr = Graphics.FromImage(_img_cache)

            If _useAA Then
                _gr.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
                _gr.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                _gr.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            End If

        End Sub

        '绘制坐标轴
        Private Sub _repaint_axis()
            Dim p As Pen
            Dim p1, p2 As Point
            If _paintXAxis Then
                p = New Pen(_XAxisColor)
                p1.X = 0
                p1.Y = _AxisCenter.Y
                p2.X = _margin.ContainerWidth
                p2.Y = _AxisCenter.Y

                If p1.X < p2.X AndAlso p1.Y < _margin.ContainerHeight Then
                    _gr.DrawLine(p, p1, p2)
                End If
            End If

            If _paintYAxis Then
                p = New Pen(_YAxisColor)
                p1.X = _AxisCenter.X
                p1.Y = 0
                p2.X = p1.X
                p2.Y = _margin.ContainerHeight

                If p1.Y < p2.Y AndAlso p1.X < _margin.ContainerWidth Then
                    _gr.DrawLine(p, p1, p2)
                End If
            End If
        End Sub

        '计算坐标轴刻度线
        Private Sub _calculate_mark()
            If _x_mark_need_update AndAlso _paintXMark AndAlso _autosizeXMark Then
                Dim delta_x As Double = _maxXAxis - _minXAxis

                If _margin.ContainerWidth > 0 AndAlso delta_x > 0 Then
                    Dim value_per_pixel As Double = delta_x / _margin.ContainerWidth
                    Dim pixel_per_value As Double = _margin.ContainerWidth / delta_x
                    Dim mark_value As Double = value_per_pixel * _minXMarkDistance '每个标记值
                    Dim mark_value2 As Double = Math.Round(mark_value, 1)
                    mark_value *= If(mark_value >= mark_value2, 1, 2)

                    _markXAxis = mark_value
                Else
                    _markXAxis = 0
                End If

                _x_mark_need_update = False
            End If
            If _y_mark_need_update AndAlso _paintYMark AndAlso _autosizeYMark Then
                Dim delta_y As Double = _maxYAxis - _minYAxis

                If _margin.ContainerHeight > 0 AndAlso delta_y > 0 Then
                    Dim value_per_pixel As Double = delta_y / _margin.ContainerHeight
                    Dim pixel_per_value As Double = _margin.ContainerHeight / delta_y
                    Dim mark_value As Double = value_per_pixel * _minYMarkDistance '每个标记值
                    Dim mark_value2 As Double = Math.Round(mark_value, 1)
                    mark_value *= If(mark_value >= mark_value2, 1, 2)

                    _markYAxis = mark_value
                Else
                    _markYAxis = 0
                End If

                _y_mark_need_update = False
            End If

        End Sub

        '计算坐标轴最值
        Private Sub _calculate_axis_value()
            '自动调整坐标下，将坐标轴的值调整为对应轨迹的最值
            If _autosizeXAxis Then
                _minXAxis = _minValue.X
                _maxXAxis = _maxValue.X
            End If
            If _autosizeYAxis Then
                _minYAxis = _minValue.Y
                _maxYAxis = _maxValue.Y
            End If

            '启用坐标交点为零点后，为保持左右两边刻度均匀，对坐标轴值进行平移/缩放处理
            If _constCentralZero Then

                'x轴最小值小于0 最大值大于0 则中间已经存在零点
                If (_minXAxis < 0 And _maxXAxis > 0) Then

                    '坐标中心在显示范围之内
                    If _AxisCenter.X > 0 AndAlso _AxisCenter.X < _margin.ContainerWidth Then
                        Dim val_per_pix1 As Double = -_minXAxis / _AxisCenter.X
                        Dim val_per_pix2 As Double = _maxXAxis / (_margin.ContainerWidth - _AxisCenter.X)
                        If val_per_pix1 > val_per_pix2 Then
                            _maxXAxis = val_per_pix1 * (_margin.ContainerWidth - _AxisCenter.X)
                        ElseIf val_per_pix1 < val_per_pix2 Then
                            _minXAxis = -val_per_pix2 * _AxisCenter.X
                        End If

                    ElseIf _AxisCenter.X <= 0 Then
                        _minXAxis = 0
                    ElseIf _AxisCenter.X >= _margin.ContainerWidth Then
                        _maxXAxis = 0
                    End If

                ElseIf _minXAxis >= 0 Then

                    If _AxisCenter.X < _margin.ContainerWidth Then
                        Dim val_per_pix As Double = _maxXAxis / (_margin.ContainerWidth - _AxisCenter.X)
                        _minXAxis = _AxisCenter.X * -val_per_pix
                    Else

                        _minXAxis = 0
                        '_maxXAxis = 0
                    End If

                ElseIf _maxXAxis <= 0 Then
                    If _AxisCenter.X > 0 Then
                        Dim val_per_pix As Double = -_minXAxis / _AxisCenter.X
                        _maxXAxis = (_margin.ContainerWidth - _AxisCenter.X) * val_per_pix
                    Else
                        '_minXAxis = 0
                        _maxYAxis = 0
                    End If
                End If


                'y轴最小值小于0 最大值大于0 则中间已经存在零点
                If (_minYAxis < 0 And _maxYAxis > 0) Then

                    '坐标中心在显示范围之内
                    If _AxisCenter.Y > 0 AndAlso _AxisCenter.Y < _margin.ContainerHeight Then
                        Dim val_per_pix1 As Double = -_minYAxis / (_margin.ContainerHeight - _AxisCenter.Y)
                        Dim val_per_pix2 As Double = _maxYAxis / _AxisCenter.Y
                        If val_per_pix1 > val_per_pix2 Then
                            _maxYAxis = val_per_pix1 * _AxisCenter.Y
                        ElseIf val_per_pix1 < val_per_pix2 Then
                            _minYAxis = -val_per_pix2 * (_margin.ContainerHeight - _AxisCenter.Y)
                        End If

                    ElseIf _AxisCenter.Y <= 0 Then
                        _maxYAxis = 0
                    ElseIf _AxisCenter.Y >= _margin.ContainerHeight Then
                        _minYAxis = 0
                    End If

                ElseIf _minYAxis >= 0 Then

                    If _AxisCenter.Y < _margin.ContainerHeight Then
                        Dim val_per_pix As Double = _maxYAxis / _AxisCenter.Y
                        _minYAxis = (_margin.ContainerHeight - _AxisCenter.Y) * -val_per_pix
                    Else

                        _minYAxis = 0
                        '_maxYAxis = 0
                    End If

                ElseIf _maxYAxis <= 0 Then
                    If _AxisCenter.Y > 0 Then
                        Dim val_per_pix As Double = -_minYAxis / (_margin.ContainerHeight - _AxisCenter.Y)
                        _maxYAxis = _AxisCenter.Y * val_per_pix
                    Else
                        '_minYAxis = 0
                        _maxYAxis = 0
                    End If
                End If
            End If


        End Sub

        '绘制坐标刻度
        Private Sub _repaint_mark()
            If _paintXMark And _markXAxis > 0 Then
                Dim mark_distance As Double = _markXAxis / (_maxXAxis - _minXAxis) * (_margin.ContainerWidth)
                If mark_distance < 1 Then mark_distance = 1

                Dim begin_pos As Double = _AxisCenter.X - Math.Floor(_AxisCenter.X / mark_distance) * mark_distance
                While begin_pos < _margin.ContainerWidth
                    Dim p1, p2 As Point
                    p1.X = begin_pos
                    p2.X = begin_pos
                    p1.Y = If(_AxisCenter.Y > 3, _AxisCenter.Y - 3, 0)
                    p2.Y = _AxisCenter.Y

                    Dim p As New Pen(_XAxisColor)
                    _gr.DrawLine(p, p1, p2)

                    If _paintXAxisValue Then
                        Dim eventArg As ExternalGetAxisValueEventArg
                        eventArg.is_XAxis = True
                        eventArg.value = begin_pos / _margin.ContainerWidth * (_maxXAxis - _minXAxis) + _minXAxis
                        eventArg.value = Math.Round(eventArg.value, 2) '保留小数
                        eventArg.display_str = eventArg.value.ToString

                        RaiseEvent ExternalGetAxisValue(eventArg)

                        If Math.Abs(begin_pos - _AxisCenter.X) >= 5 Then
                            _paint_text(eventArg.display_str, New Point(p2.X, p2.Y + 1), MarginType.Top)
                        End If
                    End If

                    begin_pos += mark_distance
                End While
            End If

            If _paintYMark And _markYAxis > 0 Then
                Dim mark_distance As Double = _markYAxis / (_maxYAxis - _minYAxis) * (_margin.ContainerHeight)
                If mark_distance < 1 Then mark_distance = 1

                Dim begin_pos As Double = _AxisCenter.Y - Math.Floor(_AxisCenter.Y / mark_distance) * mark_distance
                While begin_pos < _margin.ContainerHeight
                    Dim p1, p2 As Point
                    p1.Y = begin_pos
                    p2.Y = begin_pos
                    p1.X = If(_AxisCenter.X + 3 < _margin.ContainerWidth, _AxisCenter.X + 3, _margin.ContainerWidth)
                    p2.X = _AxisCenter.X

                    Dim p As New Pen(_YAxisColor)
                    _gr.DrawLine(p, p1, p2)

                    If _paintYAxisValue Then
                        Dim eventArg As ExternalGetAxisValueEventArg
                        eventArg.is_XAxis = False
                        eventArg.value = _maxYAxis - begin_pos / _margin.ContainerHeight * (_maxYAxis - _minYAxis)
                        eventArg.value = Math.Round(eventArg.value, 2) '保留小数
                        eventArg.display_str = eventArg.value.ToString

                        RaiseEvent ExternalGetAxisValue(eventArg)
                        If Math.Abs(begin_pos - _AxisCenter.Y) >= 5 Then
                            _paint_text(eventArg.display_str, New Point(p2.X - 1, p2.Y), MarginType.Right)
                        End If
                    End If

                    begin_pos += mark_distance
                End While
            End If
        End Sub

        '算了还是当成api格式吧
        Private Enum MarginType
            LeftTop
            Top
            RightTop
            Left
            Central
            Right
            LeftBottom
            Bottom
            RightBottom
        End Enum
        Private Sub _paint_text(ByVal text As String, ByVal point As Point, ByVal margin_type As MarginType)
            Dim range As SizeF = _gr.MeasureString(text, New Font("宋体", 9))
            Select Case margin_type
                Case MarginType.Bottom
                    _gr.DrawString(text, New Font("宋体", 9), Brushes.Black, point.X - range.Width / 2, point.Y - range.Height)
                Case MarginType.Central
                    _gr.DrawString(text, New Font("宋体", 9), Brushes.Black, point.X - range.Width / 2, point.Y - range.Height / 2)
                Case MarginType.Left
                    _gr.DrawString(text, New Font("宋体", 9), Brushes.Black, point.X, point.Y - range.Height / 2)
                Case MarginType.LeftBottom
                    _gr.DrawString(text, New Font("宋体", 9), Brushes.Black, point.X, point.Y - range.Height)
                Case MarginType.LeftTop
                    _gr.DrawString(text, New Font("宋体", 9), Brushes.Black, point.X, point.Y)
                Case MarginType.Right
                    _gr.DrawString(text, New Font("宋体", 9), Brushes.Black, point.X - range.Width, point.Y - range.Height / 2)
                Case MarginType.RightBottom
                    _gr.DrawString(text, New Font("宋体", 9), Brushes.Black, point.X - range.Width, point.Y - range.Height)
                Case MarginType.RightTop
                    _gr.DrawString(text, New Font("宋体", 9), Brushes.Black, point.X - range.Width, point.Y)
                Case MarginType.Top
                    _gr.DrawString(text, New Font("宋体", 9), Brushes.Black, point.X - range.Width / 2, point.Y)
            End Select
        End Sub

        '绘制曲线
        Private Sub _repaint_paths()
            Dim i As Integer = 0

            Dim dx As Double = _maxXAxis - _minXAxis
            Dim dy As Double = _maxYAxis - _minYAxis
            If dx <= 0 Or dy <= 0 Then Return

            Dim x_pix_per_val As Double = _margin.ContainerWidth / dx
            Dim y_pix_per_val As Double = _margin.ContainerHeight / dy


            For Each ls_path As List(Of PointD) In _paths
                Dim c As Color = _path_color(i)

                For j As Integer = 0 To ls_path.Count - 2
                    Dim cur_x As Single = x_pix_per_val * (ls_path(j).X - _minXAxis)
                    Dim cur_y As Single = y_pix_per_val * (_maxYAxis - ls_path(j).Y)
                    Dim nex_x As Single = x_pix_per_val * (ls_path(j + 1).X - _minXAxis)
                    Dim nex_y As Single = y_pix_per_val * (_maxYAxis - ls_path(j + 1).Y)

                    _gr.DrawLine(New Pen(c), cur_x, cur_y, nex_x, nex_y)
                Next

                i += 1
            Next
        End Sub

    End Class

End Namespace