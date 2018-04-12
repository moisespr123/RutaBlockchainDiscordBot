Imports System.IO
Imports System.Net
Imports System.Text
Imports DSharpPlus
Imports DSharpPlus.Entities
Imports DSharpPlus.EventArgs
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Class Form1
    Private WithEvents DiscordClient As DiscordClient
    Private Async Sub Button1_Click(sender As Object, e As System.EventArgs) Handles Button1.Click
        Button1.Text = "Running"
        Await StartAsync()
    End Sub
    Public Async Function StartAsync() As Task
        Dim dcfg As New DiscordConfiguration
        With dcfg
            .Token = My.Computer.FileSystem.ReadAllText("token.txt")
            .TokenType = TokenType.Bot
            .LogLevel = LogLevel.Debug
            .AutoReconnect = True
        End With
        Me.DiscordClient = New DiscordClient(dcfg)
        Await Me.DiscordClient.ConnectAsync()
        Await Task.Delay(-1)
    End Function
    Dim LastUserGreeted As String = My.Settings.LastUserGreeted
    Dim LastUserGoodbye As String = My.Settings.LastUserGoodbye
    Private Async Function OnGuildMemberAdd(ByVal e As GuildMemberAddEventArgs) As Task Handles DiscordClient.GuildMemberAdded
        Await DiscordClient.SendMessageAsync(Await DiscordClient.GetChannelAsync(402802330728267776), "Démosle una cordiál bienvenida a " & e.Member.Mention & " al chat de Ruta Blockchain :smiley:")
    End Function
    Private Function FindUserInFile(user As String)
        Dim userInFile As String = String.Empty
        Dim userFile As StreamReader = New StreamReader("users.txt")
        Dim currentUserLine As String = ""
        While userFile.EndOfStream = False
            currentUserLine = userFile.ReadLine
            If currentUserLine.Contains(user) Then
                Dim GetUser As String() = currentUserLine.Split("=")
                userInFile = GetUser(1)
                Exit While
            End If
        End While
        userFile.Close()
        If String.IsNullOrEmpty(userInFile) Then
            userInFile = user
        End If
        Return userInFile
    End Function
    Private Function FindUser(message As String())
        Dim UserToUse As String = String.Empty
        For Each word In message
            If word.Contains("@") Then
                If word.Contains("<") And word.Contains(">") Then
                    UserToUse = word.Remove(word.Count - 1, 1).Remove(0, 2)
                Else
                    UserToUse = word.Remove(0, 1)
                End If
            End If
        Next
        Return UserToUse
    End Function
    Private Async Function CheckUserInDiscord(user As String) As Task(Of Boolean)
        Dim UserFound As Boolean = False
        Try
            Await DiscordClient.GetUserAsync(user)
            UserFound = True
        Catch ex As Exception
            UserFound = False
        End Try
        Return UserFound
    End Function
    Private Async Function GetDiscordUser(user As String) As Task(Of DiscordUser)
        Dim UserToUse As DiscordUser = Await DiscordClient.GetUserAsync(user)
        Return UserToUse
    End Function

    Private Function GetResultFromSteemPlaceAPI(user As String, what As String)
        Dim URL As String = ""
        If what = "followers" Then
            URL = "https://api.steem.place/getFollowersCount/?a="
        ElseIf what = "following" Then
            URL = "https://api.steem.place/getFollowingCount/?a="
        ElseIf what = "location" Then
            URL = "https://api.steem.place/getLocation/?a="
        ElseIf what = "posts" Then
            URL = "https://api.steem.place/getPostCount/?a="
        ElseIf what = "creation" Then
            URL = "https://api.steem.place/getCreatedDate/?a="
        ElseIf what = "sbd" Then
            URL = "https://api.steem.place/getSBDBalance/?a="
        ElseIf what = "steem" Then
            URL = "https://api.steem.place/getSTEEMBalance/?a="
        ElseIf what = "witness" Then
            URL = "https://api.steem.place/getWitnessVotes/?a="
        ElseIf what = "vp" Then
            URL = "https://api.steem.place/getVP/?a="
        End If
        Dim myWebRequest As System.Net.WebRequest = System.Net.WebRequest.Create(URL & user)
        Dim myWebResponse As System.Net.WebResponse = myWebRequest.GetResponse()
        Dim ReceiveStream As Stream = myWebResponse.GetResponseStream()
        Dim encode As Encoding = System.Text.Encoding.GetEncoding("utf-8")
        Dim readStream As New StreamReader(ReceiveStream, encode)
        Return readStream.ReadLine
    End Function
    Private Async Function OnMessage(ByVal e As MessageCreateEventArgs) As Task Handles DiscordClient.MessageCreated
        Dim User As String = FindUserInFile(e.Message.Author.Username)
        User = User.ToLower
        Dim IsUserInDiscord As Boolean = False
        Dim UserInDiscord As DiscordUser = e.Message.Author
        If e.Message.Author.Username = "RutaBlockchainBot" = False Then
            If e.Channel.Id = 402823764586397706 Then 'ProVenezuela
                If e.Message.Content.ToLower.Contains("@") And e.Message.Content.ToLower.Contains("http") = False Then
                    Dim SplitWords As String() = e.Message.Content.Split(" ")
                    If SplitWords.Count >= 2 Then
                        Try
                            UserInDiscord = Await GetDiscordUser(FindUser(SplitWords))
                            User = FindUserInFile(UserInDiscord.Username.ToLower)
                            IsUserInDiscord = True
                        Catch
                            User = FindUser(SplitWords)
                            User = User.ToLower
                            IsUserInDiscord = False
                        End Try
                    End If
                Else
                    IsUserInDiscord = True
                End If
                If e.Message.Content.ToLower().Contains("!bot") Or e.Message.Content.ToLower().Contains("!help") Or e.Message.Content.ToLower().Contains("!ayuda") Then
                    Dim MentionMoises As DiscordUser = Await DiscordClient.GetUserAsync("323205598311219211")
                    Threading.Thread.Sleep(500)
                    Dim Message As DiscordDmChannel = Await DiscordClient.CreateDmAsync(UserInDiscord)
                    Threading.Thread.Sleep(500)
                    Await Message.SendMessageAsync("Hola, soy un bot creado por " & MentionMoises.Mention & vbCrLf & vbCrLf & "Mis comandos son los siguientes: " & vbCrLf &
                                       "!seguidores (Opcional, la persona que quieras saber sus seguidores) " & vbCrLf &
                                       "!siguiendo (Optional, la persona que quieras saber la cantidad de perfiles que sigue) " & vbCrLf &
                                       "!posts (opcional, la persona que quieras saber la cantidad de sus posts y comentarios) " & vbCrLf &
                                       "!ubicación (opcional, la persona que quieras saber la ubicación)" & vbCrLf &
                                       "!creación (opcional, la persona que quieras saber la fecha de creación de la cuenta)" & vbCrLf &
                                       "!vp (opcional, la persona que quieras saber su poder de voto)" & vbCrLf &
                                       "!steem (opcional, la persona que quieras saber sus balance de STEEM)" & vbCrLf &
                                       "!sbd (opcional, la persona que quieras saber su balance de SBD)" & vbCrLf &
                                       "!witness - Verifica si has votado a " & MentionMoises.Mention & " como Witness." & vbCrLf &
                                       "!valor (la moneda que quieras saber el valor) - Te dice el valor de la moneda que escribas" & vbCrLf &
                                       "!calcular (cantidad) (moneda) - Te calcula cuanto vale la cantidad especificada de la moneda." & vbCrLf &
                                       "!fc (temperatura) - Convierte de Fahrenheit a Celcius." & vbCrLf &
                                       "!cf (temperatura) - Convierte de Celcius a Fahrenheit." & vbCrLf &
                                       "!hoy - Te dice la fecha" & vbCrLf &
                                       "!hora (país) - Te dice la hora" & vbCrLf &
                                       "!google (terminos) - Busqueda en Google" & vbCrLf &
                                       "!wikipedia (artículo) - Busca en Wikipedia" & vbCrLf &
                                       "!perfil (usuario) - Dice el perfil tuyo o de un usuario." & vbCrLf &
                                       "!clases - muestra las clases que se realizan en el grupo" & vbCrLf &
                                       vbCrLf & "También, puedes escribir algunas cosas naturalmente, como qué hora es y qué día es hoy." & vbCrLf & vbCrLf &
                                       MentionMoises.Mention & " es un Witness. Si te gusta este bot y sus proyectos, considera votándolo como Witness :smiley:")
                    Threading.Thread.Sleep(500)
                    Await e.Channel.SendMessageAsync(UserInDiscord.Mention & ", los comandos se han enviado por mensaje privado")
                ElseIf e.Message.Content.ToLower().Contains("!clase") Then
                    Await e.Channel.SendMessageAsync("Tenemos las siguientes clases en la semana: " & vbCrLf & vbCrLf &
                                                     "Martes: Noche de poesía con @danvel" & vbCrLf &
                                                     "Miércoles: Clase de ortografía y redacción con @marynessc" & vbCrLf &
                                                     "Viernes: Tutoría de blog con @ivymalifred" &vbCrLf & vbCrLf & 
                                                     "Todas las clases son a las 7 PM hora de Venezuela")
                ElseIf e.Message.Content.ToLower().Contains("!adn") Then
                    Await e.Channel.SendMessageAsync("Mi ADN es 100% tipo Visual Basic .NET")
                ElseIf e.Message.Content.ToLower().Contains("!beso") Then
                    Await e.Channel.SendMessageAsync(":kissing_heart:")
                ElseIf e.Message.Content.ToLower().Contains("!besito") Then
                    Await e.Channel.SendMessageAsync(":kissing_smiling_eyes:")
                ElseIf e.Message.Content.ToLower().Contains("!enamorar") Then
                    Await e.Channel.SendMessageAsync(":kissing_heart: :kissing_heart: :kissing_heart: :kissing_heart: :kissing_heart: :kissing_heart: :kissing_heart:")
                ElseIf e.Message.Content.ToLower().Contains("hola") And e.Message.Content.Contains("@") = False Then
                    If LastUserGreeted <> e.Message.Author.Username Then
                        Await e.Channel.SendMessageAsync("Hola, " & UserInDiscord.Mention)
                        LastUserGreeted = e.Message.Author.Username
                        SaveGreetedUser(LastUserGreeted, LastUserGoodbye)
                    End If
                ElseIf e.Message.Content.ToLower().Contains("saludos") And e.Message.Content.Contains("@") = False Then
                    If LastUserGreeted <> e.Message.Author.Username Then
                        Await e.Channel.SendMessageAsync("Saludos, " & UserInDiscord.Mention)
                        LastUserGreeted = e.Message.Author.Username
                        SaveGreetedUser(LastUserGreeted, LastUserGoodbye)
                    End If
                ElseIf e.Message.Content.ToLower().Contains("buenos dias") Or e.Message.Content.ToLower().Contains("buenos días") And e.Message.Content.Contains("@") = False Then
                    If LastUserGreeted <> e.Message.Author.Username Then
                        Await e.Channel.SendMessageAsync("Buenos Días, " & UserInDiscord.Mention)
                        LastUserGreeted = e.Message.Author.Username
                        SaveGreetedUser(LastUserGreeted, LastUserGoodbye)
                    End If
                ElseIf e.Message.Content.ToLower().Contains("buen dia") Or e.Message.Content.ToLower().Contains("buen día") And e.Message.Content.Contains("@") = False Then
                    If LastUserGreeted <> e.Message.Author.Username Then
                        Await e.Channel.SendMessageAsync("Buen Día, " & UserInDiscord.Mention)
                        LastUserGreeted = e.Message.Author.Username
                        SaveGreetedUser(LastUserGreeted, LastUserGoodbye)
                    End If
                ElseIf e.Message.Content.ToLower().Contains("buenas tardes") And e.Message.Content.Contains("@") = False Then
                    If LastUserGreeted <> e.Message.Author.Username Then
                        Await e.Channel.SendMessageAsync("Buenas Tardes, " & UserInDiscord.Mention)
                        LastUserGreeted = e.Message.Author.Username
                        SaveGreetedUser(LastUserGreeted, LastUserGoodbye)
                    End If
                ElseIf e.Message.Content.ToLower().Contains("buenas noches") And e.Message.Content.Contains("@") = False Then
                    If LastUserGreeted <> e.Message.Author.Username Then
                        Await e.Channel.SendMessageAsync("Buenas Noches, " & UserInDiscord.Mention)
                        LastUserGreeted = e.Message.Author.Username
                        SaveGreetedUser(LastUserGreeted, LastUserGoodbye)
                    End If
                ElseIf (e.Message.Content.ToLower().Contains("feliz noche") Or e.Message.Content.ToLower().Contains("felíz noche")) And e.Message.Content.Contains("@") = False Then
                    If LastUserGreeted <> e.Message.Author.Username Then
                        Await e.Channel.SendMessageAsync("Felíz Noche, " & UserInDiscord.Mention)
                        LastUserGreeted = e.Message.Author.Username
                        SaveGreetedUser(LastUserGreeted, LastUserGoodbye)
                    End If
                ElseIf e.Message.Content.ToLower().Contains("buenas") And e.Message.Content.Contains("@") = False Then
                    If LastUserGreeted <> e.Message.Author.Username Then
                        Await e.Channel.SendMessageAsync("Buenas, " & UserInDiscord.Mention)
                        LastUserGreeted = e.Message.Author.Username
                        SaveGreetedUser(LastUserGreeted, LastUserGoodbye)
                    End If
                ElseIf (e.Message.Content.ToLower().Contains("adios") Or e.Message.Content.ToLower().Contains("adiós") Or e.Message.Content.ToLower().Contains("me voy") Or e.Message.Content.ToLower().Contains("me ire") Or e.Message.Content.ToLower().Contains("me iré") Or e.Message.Content.ToLower().Contains("me fui") Or e.Message.Content.ToLower().Contains("me fuí") Or e.Message.Content.ToLower().Contains("los dejo por hoy") Or e.Message.Content.ToLower().Contains("hasta luego") Or e.Message.Content.ToLower().Contains("nas noches")) And e.Message.Content.Contains("@") = False Then
                    If LastUserGoodbye <> e.Message.Author.Username Then
                        Await e.Channel.SendMessageAsync("Adiós, " & UserInDiscord.Mention)
                        LastUserGoodbye = e.Message.Author.Username
                        SaveGoodbyeUser(LastUserGoodbye, LastUserGreeted)
                    End If
                ElseIf e.Message.Content.ToLower().Contains("chao") And e.Message.Content.Contains("@") = False Then
                    If LastUserGoodbye <> e.Message.Author.Username Then
                        Await e.Channel.SendMessageAsync("Chao, " & UserInDiscord.Mention)
                        LastUserGoodbye = e.Message.Author.Username
                        SaveGoodbyeUser(LastUserGoodbye, LastUserGreeted)
                    End If
                End If
                If e.Message.Content.ToLower().Contains("!ping") Then
                    Await e.Channel.SendMessageAsync("pong")
                ElseIf e.Message.Content.ToLower().Contains("buena noticia") Or e.Message.Content.ToLower().Contains("buenas noticias") Then
                    Await e.Channel.SendMessageAsync("¡Enhorabuena!")
                ElseIf e.Message.Content.ToLower().Contains("!seguidores") Then
                    Dim FollowerNumber As Integer = GetResultFromSteemPlaceAPI(User, "followers")
                    If IsUserInDiscord Then
                        Await e.Channel.SendMessageAsync(UserInDiscord.Mention & ", tienes " & FollowerNumber & " seguidores :smiley:")
                    Else
                        Await e.Channel.SendMessageAsync("@" & User & " tiene " & FollowerNumber & " seguidores :smiley:")
                    End If
                ElseIf e.Message.Content.ToLower().Contains("!siguiendo") Then
                    Dim FollowingNumber As Integer = GetResultFromSteemPlaceAPI(User, "following")
                    If IsUserInDiscord Then
                        Await e.Channel.SendMessageAsync(UserInDiscord.Mention & ", sigues a " & FollowingNumber & " perfiles :smiley:")
                    Else
                        Await e.Channel.SendMessageAsync("@" & User & " sigue a " & FollowingNumber & " perfiles :smiley:  ")
                    End If
                ElseIf e.Message.Content.ToLower().Contains("!ubicación") Or e.Message.Content.ToLower.Contains("!ubicacion") Then
                    Dim location As String = GetResultFromSteemPlaceAPI(User, "location")
                    If IsUserInDiscord Then
                        Await e.Channel.SendMessageAsync(UserInDiscord.Mention & " es de " & location & " :smiley:")
                    Else
                        Await e.Channel.SendMessageAsync("@" & User & " es de " & location & " :smiley:")
                    End If
                ElseIf e.Message.Content.ToLower().Contains("!posts") Then
                    Dim postNumber As Integer = GetResultFromSteemPlaceAPI(User, "posts")
                    If IsUserInDiscord Then
                        Await e.Channel.SendMessageAsync(UserInDiscord.Mention & ", tienes " & postNumber & " posts y comentarios combinados :smiley:")
                    Else
                        Await e.Channel.SendMessageAsync("@" & User & " tiene " & postNumber & " posts y comentarios combinados :smiley:")
                    End If
                ElseIf e.Message.Content.ToLower().Contains("!creacion") Or e.Message.Content.ToLower().Contains("!creación") Or e.Message.Content.ToLower().Contains("!registro") Then
                    Dim DateAndTime As Date = GetResultFromSteemPlaceAPI(User, "creation")
                    Dim GetMonth As Integer = DateAndTime.ToString("MM")
                    Dim MonthName As String = String.Empty
                    If GetMonth = 1 Then
                        MonthName = "enero"
                    ElseIf GetMonth = 2 Then
                        MonthName = "febrero"
                    ElseIf GetMonth = 3 Then
                        MonthName = "marzo"
                    ElseIf GetMonth = 4 Then
                        MonthName = "abril"
                    ElseIf GetMonth = 5 Then
                        MonthName = "mayo"
                    ElseIf GetMonth = 6 Then
                        MonthName = "junio"
                    ElseIf GetMonth = 7 Then
                        MonthName = "julio"
                    ElseIf GetMonth = 8 Then
                        MonthName = "agosto"
                    ElseIf GetMonth = 9 Then
                        MonthName = "septiembre"
                    ElseIf GetMonth = 10 Then
                        MonthName = "octubre"
                    ElseIf GetMonth = 11 Then
                        MonthName = "noviembre"
                    ElseIf GetMonth = 12 Then
                        MonthName = "diciembre"
                    End If
                    Dim FullDate As String = Convert.ToInt32(DateAndTime.ToString("dd")).ToString + " de " + MonthName + " de " + DateAndTime.ToString("yyyy")
                    If IsUserInDiscord Then
                        Await e.Channel.SendMessageAsync(UserInDiscord.Mention & ", tu cuenta fue creada el " & FullDate & " a las " & DateAndTime.ToString("hh:mm:ss tt") & " :smiley: ")
                    Else
                        Await e.Channel.SendMessageAsync("La cuenta de @" & User & " fue creada el " & FullDate & " a las " & DateAndTime.ToString("hh:mm:ss tt") & " :smiley: ")
                    End If
                ElseIf e.Message.Content.ToLower().Contains("!sbd") Then
                    Dim Balance As String = GetResultFromSteemPlaceAPI(User, "sbd")
                    If IsUserInDiscord Then
                        Await e.Channel.SendMessageAsync(UserInDiscord.Mention & ", tienes " & Balance & " :smiley: ")
                    Else
                        Await e.Channel.SendMessageAsync("@" & User & " tiene " & Balance & " :smiley: ")
                    End If
                ElseIf e.Message.Content.ToLower().Contains("!steem") Then
                    Dim Balance As String = GetResultFromSteemPlaceAPI(User, "steem")
                    If IsUserInDiscord Then
                        Await e.Channel.SendMessageAsync(UserInDiscord.Mention & ", tienes " & Balance & " :smiley: ")
                    Else
                        Await e.Channel.SendMessageAsync("@" & User & " tiene " & Balance & " :smiley: ")
                    End If
                ElseIf e.Message.Content.ToLower().Contains("!vp") Then
                    Dim VP As Double = GetResultFromSteemPlaceAPI(User, "vp")
                    If IsUserInDiscord Then
                        Await e.Channel.SendMessageAsync(UserInDiscord.Mention & ", tu poder de voto es " & VP & "% :slight_smile: ")
                    Else
                        Await e.Channel.SendMessageAsync("@" & User & ", tu poder de voto es " & VP & "% :slight_smile: ")
                    End If
                ElseIf e.Message.Content.ToLower().Contains("!witness") Then
                    Dim WitnessVotes As String = GetResultFromSteemPlaceAPI(User, "witness")
                    Dim VoteLink As String = ""
                    Dim Voted = False
                    Dim MentionMoises As DiscordUser = Await DiscordClient.GetUserAsync("323205598311219211")
                    If String.IsNullOrEmpty(WitnessVotes) = False Then
                        If WitnessVotes.Contains("moisesmcardona") Then
                            If IsUserInDiscord = True Then
                                Await e.Channel.SendMessageAsync(UserInDiscord.Mention & ", has votado a " & MentionMoises.Mention & " como Witness :smiley: ")
                            Else
                                Await e.Channel.SendMessageAsync(User & " ha votado a " & MentionMoises.Mention & " como Witness :smiley: ")
                            End If
                        Else
                            If IsUserInDiscord = True Then
                                Await e.Channel.SendMessageAsync(UserInDiscord.Mention & ", no has votado a " & MentionMoises.Mention & " como Witness :cry: Vótalo usando el siguiente enlace: https://v2.steemconnect.com/sign/account-witness-vote?witness=moisesmcardona&approve=1")
                            Else
                                Await e.Channel.SendMessageAsync(User & " no ha votado a " & MentionMoises.Mention & " como Witness :cry:")
                            End If
                        End If
                    End If
                ElseIf e.Message.Content.ToLower.Contains("!fc") Then
                    Dim SplitWords As String() = e.Message.Content.Split(" ")
                    If SplitWords.Count >= 2 Then
                        Dim Temperatura As Double = 0.0
                        For currentword = 1 To SplitWords.Count - 1
                            Temperatura += SplitWords(currentword)
                        Next
                        Dim Converted As Double = 0.0
                        Converted = (Temperatura - 32) * (5 / 9)
                        Await e.Channel.SendMessageAsync(Temperatura & " F equivale a " & Converted & " C")
                    Else
                        Await e.Channel.SendMessageAsync("Para usar esta función, tienes que escribir los grados para convertir a Celcius")
                    End If
                ElseIf e.Message.Content.ToLower.Contains("!cf") Then
                    Dim SplitWords As String() = e.Message.Content.Split(" ")
                    If SplitWords.Count >= 2 Then
                        Dim Temperatura As Double = 0.0
                        For currentword = 1 To SplitWords.Count - 1
                            Temperatura += SplitWords(currentword)
                        Next
                        Dim Converted As Double = 0.0
                        Converted = (Temperatura * (9 / 5)) + 32
                        Await e.Channel.SendMessageAsync(Temperatura & " C equivale a " & Converted & " F")
                    Else
                        Await e.Channel.SendMessageAsync("Para usar esta función, tienes que escribir los grados para convertir a Fahrenheit")
                    End If
                ElseIf e.Message.Content.ToLower.Contains("!hoy") Or e.Message.Content.ToLower.Contains("que dia es hoy?") Or e.Message.Content.ToLower.Contains("qué día es hoy?") Or e.Message.Content.ToLower.Contains("qué dia es hoy?") Or e.Message.Content.ToLower.Contains("que día es hoy?") Then
                    Dim Month As String = Date.Today.ToString("MM")
                    Dim MonthName As String = ""
                    If Month = "01" Then
                        MonthName = "enero"
                    ElseIf Month = "02" Then
                        MonthName = "febrero"
                    ElseIf Month = "03" Then
                        MonthName = "marzo"
                    ElseIf Month = "04" Then
                        MonthName = "abril"
                    ElseIf Month = "05" Then
                        MonthName = "mayo"
                    ElseIf Month = "06" Then
                        MonthName = "junio"
                    ElseIf Month = "07" Then
                        MonthName = "julio"
                    ElseIf Month = "08" Then
                        MonthName = "agosto"
                    ElseIf Month = "09" Then
                        MonthName = "septiembre"
                    ElseIf Month = "10" Then
                        MonthName = "octubre"
                    ElseIf Month = "11" Then
                        MonthName = "noviembre"
                    ElseIf Month = "12" Then
                        MonthName = "diciembre"
                    End If
                    Dim Day As String = Date.Today.ToString("dddd")
                    Dim DayName As String = ""
                    If Day.ToLower = "monday" Then
                        DayName = "lunes"
                    ElseIf Day.ToLower = "tuesday" Then
                        DayName = "martes"
                    ElseIf Day.ToLower = "wednesday" Then
                        DayName = "miércoles"
                    ElseIf Day.ToLower = "thursday" Then
                        DayName = "jueves"
                    ElseIf Day.ToLower = "friday" Then
                        DayName = "viernes"
                    ElseIf Day.ToLower = "saturday" Then
                        DayName = "sábado"
                    ElseIf Day.ToLower = "sunday" Then
                        DayName = "domingo"
                    End If
                    Await e.Channel.SendMessageAsync("Hoy es " & DayName & ", " & Date.Today.ToString("dd") & " de " & MonthName & " de " & Date.Today.ToString("yyyy"))
                ElseIf e.Message.Content.ToLower.Contains("!hora") Or e.Message.Content.ToLower.Contains("que hora es?") Or e.Message.Content.ToLower.Contains("qué hora es?") Or e.Message.Content.ToLower.Contains("qué hora es en") Or e.Message.Content.ToLower.Contains("que hora es en") Then
                    If e.Message.Content.ToLower.Contains("españa") Or e.Message.Content.ToLower.Contains("madrid") Or e.Message.Content.ToLower.Contains("brussels") Or e.Message.Content.ToLower.Contains("Copenhagen") Then
                        Dim localZone As DateTimeOffset = TimeZoneInfo.ConvertTime(Date.Now, TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time"))
                        Await e.Channel.SendMessageAsync("Son las " & localZone.ToString("HH:mm") & " en España. Zona de tiempo: UTC+01:00 Romance Standard Time: Brussels, Copenhagen, Madrid, Paris")
                    ElseIf e.Message.Content.ToLower.Contains("paris") Then
                        Dim localZone As DateTimeOffset = TimeZoneInfo.ConvertTime(Date.Now, TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time"))
                        Await e.Channel.SendMessageAsync("Son las " & localZone.ToString("HH:mm") & " en París. Zona de tiempo: UTC+01:00 Romance Standard Time: Brussels, Copenhagen, Madrid, Paris")

                    ElseIf e.Message.Content.ToLower.Contains("argentina") Or e.Message.Content.ToLower.Contains("buenos aires") Then
                        Dim localZone As DateTimeOffset = TimeZoneInfo.ConvertTime(Date.Now, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time"))
                        Await e.Channel.SendMessageAsync("Son las " & localZone.ToString("HH:mm") & " en Argentina. Zona de tiempo: UTC-03:00 Argentina Standard Time: Buenos Aires")
                    ElseIf e.Message.Content.ToLower.Contains("venezuela") Or e.Message.Content.ToLower.Contains("caracas") Then
                        Dim localZone As DateTimeOffset = TimeZoneInfo.ConvertTime(Date.Now, TimeZoneInfo.FindSystemTimeZoneById("Venezuela Standard Time"))
                        Await e.Channel.SendMessageAsync("Son las " & localZone.ToString("HH:mm") & " en Venezuela. Zona de tiempo: UTC-04:00 Venezuela Standard Time: Caracas")
                    ElseIf e.Message.Content.ToLower.Contains("mexico") Or e.Message.Content.ToLower.Contains("méxico") Or e.Message.Content.ToLower.Contains("guadalajara") Or e.Message.Content.ToLower.Contains("monterrey") Then
                        Dim localZone As DateTimeOffset = TimeZoneInfo.ConvertTime(Date.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)"))
                        Await e.Channel.SendMessageAsync("Son las " & localZone.ToString("HH:mm") & " en México (Central). Zona de tiempo: UTC-06:00 Central Standard Time (Mexico): Guadalajara, Mexico City, Monterrey")
                    ElseIf e.Message.Content.ToLower.Contains("italia") Or e.Message.Content.ToLower.Contains("amsterdam") Or e.Message.Content.ToLower.Contains("berlin") Or e.Message.Content.ToLower.Contains("bern") Or e.Message.Content.ToLower.Contains("roma") Or e.Message.Content.ToLower.Contains("stockholm") Or e.Message.Content.ToLower.Contains("vienna") Then
                        Dim localZone As DateTimeOffset = TimeZoneInfo.ConvertTime(Date.Now, TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"))
                        Await e.Channel.SendMessageAsync("Son las " & localZone.ToString("HH:mm") & " en Italia. Zona de tiempo: UTC+01:00 Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna")
                    Else
                        Dim localZone As DateTimeOffset = TimeZoneInfo.ConvertTime(Date.Now, TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time"))
                        Await e.Channel.SendMessageAsync("Son las " & localZone.ToString("hh:mm tt") & ". Zona de tiempo: UTC-04:00 SA Western Standard Time: Georgetown, La Paz, Manaus, San Juan")
                    End If
                ElseIf e.Message.Content.ToLower.Contains("!google") Then
                    Dim SplitWords As String() = e.Message.Content.Split(" ")
                    If SplitWords.Count >= 2 Then
                        Dim GoogleSearch As String = ""
                        For currentword = 1 To SplitWords.Count - 1
                            GoogleSearch += SplitWords(currentword) & " "
                        Next
                        Dim GoogleSearchURL = GoogleSearch.Replace(" ", "%20")
                        Await e.Channel.SendMessageAsync("https://google.com/search?q=" & GoogleSearchURL.Remove(GoogleSearchURL.Count - 3, 3))
                    Else
                        Await e.Channel.SendMessageAsync("Para usar esta función, tienes que escribir un término luego de la palabra google.")
                    End If
                ElseIf e.Message.Content.ToLower.Contains("!wiki") Then
                    Dim SplitWords As String() = e.Message.Content.Split(" ")
                    If SplitWords.Count >= 2 Then
                        Dim WikiArticle As String = ""
                        For currentword = 1 To SplitWords.Count - 1
                            WikiArticle += SplitWords(currentword) & " "
                        Next
                        Dim ArticleResult As String = GetWikipediaArticle(WikiArticle)
                        If String.IsNullOrEmpty(ArticleResult) = False Then
                            Await e.Channel.SendMessageAsync(ArticleResult & vbNewLine & vbNewLine & "Más del articulo en: " & "https://es.wikipedia.org/wiki/" & WikiArticle.Replace(" ", "_").Remove(WikiArticle.Count - 1, 1))
                        Else
                            Await e.Channel.SendMessageAsync("No se ha encontrado un artículo con esos términos. Verifica que los términos estén bien escritos (Mayúsculas, acentos...) ")
                        End If
                    Else
                        Await e.Channel.SendMessageAsync("Para usar esta función, tienes que escribir un término luego de la palabra wikipedia.")
                    End If
                ElseIf e.Message.Content.ToLower.Contains("!perfil") Then
                    If IsUserInDiscord Then
                        Await e.Channel.SendMessageAsync("El perfil de " & UserInDiscord.Mention & " es: " & vbCrLf & "en Steemit: https://steemit.com/@" & User & vbCrLf & "en Busy: https://busy.org/@" & User)
                    Else

                        Await e.Channel.SendMessageAsync("El perfil de @" & User & " es: " & vbCrLf & "en Steemit: https://steemit.com/@" & User & vbCrLf & "en Busy: https://busy.org/@" & User)
                    End If
                End If
            End If
            'If e.Channel.Id = "" Then
            '    If e.Message.Content.ToLower().Contains("!valor") Then
            '    Dim Reply As String = String.Empty
            '    Dim SplitWords As String() = e.Message.Content.Split(" ")
            '    If SplitWords.Count >= 2 Then
            '        Dim i = 0
            '        For Each word In SplitWords
            '            If word = "!valor" Then
            '                Reply = GetOrCalculatePrice(SplitWords(i + 1))
            '            End If
            '            i = i + 1
            '        Next
            '        Await e.Channel.SendMessageAsync(Reply)
            '    Else
            '        Reply = GetOrCalculatePrice("steem", "USD")
            '        Await e.Channel.SendMessageAsync(Reply)
            '    End If
            'ElseIf e.Message.Content.ToLower().Contains("!calcular") Then
            '    Dim Reply As String = String.Empty
            '        Dim SplitWords As String() = e.Message.Content.Split(" ")
            '        If SplitWords.Count >= 2 Then
            '            Dim i = 0
            '            For Each word In SplitWords
            '                If word = "!calcular" Then
            '                    Reply = GetOrCalculatePrice(SplitWords(i + 2), SplitWords(i + 1))
            '                End If
            '                i = i + 1
            '            Next
            '            Await e.Channel.SendMessageAsync(Reply)
            '        Else
            '            Reply = GetOrCalculatePrice("steem", "USD")
            '            Await e.Channel.SendMessageAsync(Reply)
            '        End If
            '    End If
            'End If
            '403626371659726879 = #post-utiles-de-steemit
            '403636422612877312 = #anuncios
            '404108969905356801 = #steem-walking
            '404781212536799233 = #meetup
            '413455484213919744 = #tutorias
            '412055031370612736 = #debates-domingueros
            '408693599077924864 = #evangecriptos
            '402820779164696577 = #publica tu post
            'If e.Channel.Id = 403626371659726879 = False And e.Channel.Id = 403636422612877312 = False And e.Channel.Id = 404108969905356801 = False And e.Channel.Id = 404781212536799233 = False And e.Channel.Id = 413455484213919744 = False And e.Channel.Id = 412055031370612736 = False And e.Channel.Id = 408693599077924864 = False and e.Channel.Id = 402820779164696577 = False Then
            '    If e.Message.Content.Contains("steemit.com/") Or e.Message.Content.Contains("busy.org/") Or e.Message.Content.Contains("utopian.io/") Then
            '        Dim SplitWords As String() = e.Message.Content.Split("/")
            '        If SplitWords.Count > 4 Then
            '            Dim Channel As DiscordChannel = Await DiscordClient.GetChannelAsync(402820779164696577)
            '            Dim Message As String = ":police_car: :rotating_light: :police_car: :rotating_light: :police_car: :rotating_light: :police_car: :rotating_light: :police_car: :rotating_light: :police_car: :rotating_light: " & vbCrLf &
            '                                    ":warning: " & UserInDiscord.Mention & " ADVERTENCIA " & UserInDiscord.Mention & " :warning:" & vbCrLf &
            '                                    ":rotating_light: Mensaje con post detectado :rotating_light:" & vbCrLf &
            '                                    ":rotating_light: En este canal, no se permiten posts. Los posts deben ir en el canal " & Channel.Mention & " :rotating_light:" & vbCrLf &
            '                                    ":police_car: :rotating_light: :police_car: :rotating_light: :police_car: :rotating_light: :police_car: :rotating_light: :police_car: :rotating_light: :police_car: :rotating_light: " & vbCrLf &
            '                                    ":warning: Por favor, escribe para confirmar que viste la advertencia :warning: Infracciones múltiples resultarán en tu expulsión de este servidor."
            '            Await e.Channel.SendMessageAsync(Message)
            '        End If
            '    End If
            'End If
        End If
    End Function
    Private Sub SaveGreetedUser(LastUserGreeted As String, LastUserGoodbye As String)
        If LastUserGoodbye = LastUserGreeted Then
            LastUserGoodbye = ""
            Me.LastUserGoodbye = ""
            My.Settings.LastUserGoodbye = LastUserGoodbye
        End If
        My.Settings.LastUserGreeted = LastUserGreeted
        My.Settings.Save()
    End Sub
    Private Sub SaveGoodbyeUser(LastUserGoodbye As String, LastUserGreeted As String)
        If LastUserGreeted = LastUserGoodbye Then
            LastUserGreeted = ""
            Me.LastUserGreeted = ""
            My.Settings.LastUserGreeted = LastUserGreeted
        End If
        My.Settings.LastUserGoodbye = LastUserGoodbye
        My.Settings.Save()
    End Sub
    Private Function GetOrCalculatePrice(Currency As String, Optional Amount As Double = 0.0)
        Dim CurrencyReply As String = String.Empty
        Dim request As System.Net.WebRequest = System.Net.WebRequest.Create("https://api.coinmarketcap.com/v1/ticker/?limit=1000")
        Dim response As System.Net.WebResponse = request.GetResponse()
        Console.WriteLine(CType(response, HttpWebResponse).StatusDescription)
        Dim dataStream As Stream = response.GetResponseStream()
        Dim reader As New StreamReader(dataStream)
        Dim responseFromServer As String = reader.ReadToEnd()
        Dim result = JsonConvert.DeserializeObject(Of ArrayList)(responseFromServer)
        Dim token As JToken
        Dim id As String = ""
        Dim name As String = ""
        Dim symbol As String = ""
        Dim price_usd As String = ""
        Dim price_btc As String = ""
        Dim percent_1h As String = ""
        Dim percent_24h As String = ""
        Dim percent_7d As String = ""
        For Each value As Object In result
            token = JObject.Parse(value.ToString())
            id = token.SelectToken("id")
            name = token.SelectToken("name")
            symbol = token.SelectToken("symbol")
            If id.ToLower = Currency.ToLower Or name.ToLower = Currency.ToLower Or symbol.ToLower = Currency.ToLower Then
                price_usd = token.SelectToken("price_usd")
                price_btc = token.SelectToken("price_btc")
                percent_1h = token.SelectToken("percent_change_1h")
                percent_24h = token.SelectToken("percent_change_24h")
                percent_7d = token.SelectToken("percent_change_7d")
                Exit For
            End If
        Next
        reader.Close()
        response.Close()
        Dim CurrencyName As String = "dólares"
        If price_btc = String.Empty Then
            CurrencyReply = "No se ha encontrado el valor de la moneda " & Currency & "."
        Else
            If Amount = 0.0 Then
                CurrencyReply = "El valor de " & name & " es " & price_btc & " BTC y $" & price_usd & " " & CurrencyName & ". " & vbCrLf & "Porcientos de cambio:" & vbCrLf & "Pasada hora: " & percent_1h & "%" & vbCrLf & "Pasadas 24 horas: " & percent_24h & "%" & vbCrLf & "Pasados 7 días: " & percent_7d & "%" & vbCrLf & "Fuente: CoinMarketCap"
            Else
                Dim BTCCalc = Amount * price_btc
                Dim USDCalc = Amount * price_usd
                CurrencyReply = Amount & " " & name & " equivalen a " & BTCCalc & " BTC y $" & USDCalc & " " & CurrencyName & ". Basado en los resultados actuales de CoinMarketCap"
            End If
        End If
        Return CurrencyReply
    End Function
    Private Function GetWikipediaArticle(article As String)
        Dim WikipediaText As String = ""
        Try
            Dim ResponseFromServer As String = String.Empty
            Dim request As System.Net.WebRequest = System.Net.WebRequest.Create("https://es.wikipedia.org/wiki/" & article)
            Dim response As System.Net.WebResponse = request.GetResponse()
            request.Timeout = 10 * 1000
            Dim dataStream As Stream = response.GetResponseStream()
            Dim reader As New StreamReader(dataStream)
            ResponseFromServer = reader.ReadToEnd()
            Dim parse1 As CsQuery.CQ = CsQuery.CQ.Create(ResponseFromServer, CsQuery.HtmlParsingMode.Auto, CsQuery.HtmlParsingOptions.IgnoreComments)
            Dim parseArticle As CsQuery.CQ = parse1("div.mw-parser-output > p")
            Dim TempText = parseArticle.First.Text
            WikipediaText = TempText.Replace(vbLf, String.Empty)
        Catch
            WikipediaText = String.Empty
        End Try
        Return WikipediaText
    End Function
    Private Async Sub Button4_Click(sender As Object, e As System.EventArgs) Handles Button4.Click
        Dim UserInDiscord As DiscordUser = Await DiscordClient.GetUserAsync(TextBox1.Text)
        Dim Message As String =
":police_car: :rotating_light: :police_car: :rotating_light: :police_car: :rotating_light: :police_car: :rotating_light: :police_car: :rotating_light: :police_car: :rotating_light: " & vbCrLf &
":warning: " & UserInDiscord.Mention & " ADVERTENCIA " & UserInDiscord.Mention & " :warning:" & vbCrLf &
":rotating_light: Mensaje con sólamente el post detectado :rotating_light:" & vbCrLf &
":rotating_light: En este canal, no se permiten posts. Por favor, utiliza el canal de promoción para promocionar tu post :rotating_light:" & vbCrLf &
":police_car: :rotating_light: :police_car: :rotating_light: :police_car: :rotating_light: :police_car: :rotating_light: :police_car: :rotating_light: :police_car: :rotating_light: " & vbCrLf &
":warning: Por favor, escribe para confirmar que viste la advertencia :warning: Infracciones múltiples resultarán en tu expulsión de este servidor."
        Await DiscordClient.SendMessageAsync(Await DiscordClient.GetChannelAsync(402823764586397706), Message)
    End Sub

    Private Async Sub Button3_Click(sender As Object, e As System.EventArgs) Handles Button3.Click
        Await DiscordClient.SendMessageAsync(Await DiscordClient.GetChannelAsync(402823764586397706), TextBox1.Text)
    End Sub
End Class
