using Moq;
namespace TestDoublesExercise.Tests;

public sealed class ReservationServiceTests
{
    // Entfernt Skip jeweils erst, wenn ihr den Test implementiert habt.
    // Die Beispiele für Stub, Spy und Moq stehen in TestDoubleExamples.cs.

    private const string Email = "ruediddysueli.@gmail.com";
    private const string Code = "KONZERT-67";

    // Aufgabe 1:
    // ReservationService hängt von der Schnittstelle ISeatAvailability ab statt ner konkreten Klasse, Abhängigkeit wird durch Dependency Injection übergeben.
    // Deshalb wird im Testprojekt ein eigener Stub geschrieben, der die Schnittstelle implementiert und der Service direkt getestet wird.
    // Die Schnittstelle erlaubt bereits genau eine Methode GetAvailableSeats(string eventCode), Anzahl Plätze wird zu int umgewandelt, und Result wird als richtig zurückgegeben wenns gleich oder grösser als seatCount ist, das gegenteil wenn kleiner.

    // GetAvailableSeats liefert die Anzahl der momentan freier Plätze für genau diese Veranstaltung bereits reservierte Plätze sind schon abgezogen, der Wert ist nie negativ.
    // Für einen unbekannten Veranstaltungscode liefert sie 0 und wirft keine Exception.

    // SeatAvailabilityStub
    // Der festgelegte Veranstaltungscode gibt die vorgegebene Anzahl zurück, sonst 0.
    // Er berechnet keine echte Verfügbarkeit.

    [Fact]
    public void Reserve_WithEnoughSeats_ReturnsTrue()
    {
        // Arrange: Stub mit 3 freien Plätzen, Repository mit erfolgreichem Speichern,
        //          eigener Sender-Spy und ReservationService.
        var availability = new SeatAvailabilityStub(Code, 3);
        var repository = new ReservationRepositoryDouble(saveSucceeds: true);
        var sender = new ConfirmationSenderSpy();
        var service = new ReservationService(availability, repository, sender);

        // Act:     2 Plätze reservieren.
        var result = service.Reserve(Email, Code, 2);

        // Assert:  Ergebnis ist true.
        Assert.True(result);
    }

    [Fact]
    public void Reserve_WithTooFewSeats_DoesNotSaveOrSend()
    {
        // Arrange: Stub mit 1 freiem Platz und Doubles für Repository und Sender.
        var availability = new SeatAvailabilityStub(Code, 1);
        var repository = new ReservationRepositoryDouble(saveSucceeds: true);
        var sender = new ConfirmationSenderSpy();
        var service = new ReservationService(availability, repository, sender);

        // Act:     2 Plätze reservieren.
        var result = service.Reserve(Email, Code, 2);

        // Assert:  Ergebnis ist false; weder Speichern noch Senden wurde aufgerufen.
        Assert.False(result);
        Assert.Empty(repository.SaveAttempts);
        Assert.Empty(sender.SentConfirmations);
    }

    [Fact]
    public void Reserve_WithExactlyEnoughSeats_Succeeds()
    {
        // Arrange: Stub mit genau 2 freien Plätzen.
        var availability = new SeatAvailabilityStub(Code, 2);
        var repository = new ReservationRepositoryDouble(saveSucceeds: true);
        var sender = new ConfirmationSenderSpy();
        var service = new ReservationService(availability, repository, sender);

        // Act:     2 Plätze reservieren.
        var result = service.Reserve(Email, Code, 2);

        // Assert:  Ergebnis ist true.
        Assert.True(result);
    }


    // Aufgabe 2:
    // Bei falschen Eingaben soll der Service keine Abhängigkeit aufrufen.
    // Ein Aufruf, der nicht existiert, hinterlässt aber keine Spur im Resultat.
    // Darum lassen wir die Doubles mitzählen.
    //
    // Der Stub zählt durch AvailabilityRequests, das Repository Double merkt sich jeden Speicherversuch und der Spy jede Bestätigung durch SentConfirmations.
    // Nach der Exception wird geprüft, dass alle drei leer/0 sind.
    // Der Produktionscode bleibt derselbe.

    [Fact]
    public void Reserve_WithZeroSeats_ThrowsBeforeCallingDependencies()
    {
        // Arrange: Doubles, deren Aufrufe sich prüfen lassen.
        var availability = new SeatAvailabilityStub(Code, 3);
        var repository = new ReservationRepositoryDouble(saveSucceeds: true);
        var sender = new ConfirmationSenderSpy();
        var service = new ReservationService(availability, repository, sender);

        // Act:     0 Plätze reservieren.
        var exception = Record.Exception(() => service.Reserve(Email, Code, 0));

        // Assert:  ArgumentOutOfRangeException; keine Abhängigkeit wurde aufgerufen.
        var argumentException = Assert.IsType<ArgumentOutOfRangeException>(exception);
        Assert.Equal("seatCount", argumentException.ParamName);
        Assert.Equal(0, availability.AvailabilityRequests);
        Assert.Empty(repository.SaveAttempts);
        Assert.Empty(sender.SentConfirmations);
    }

    [Fact]
    public void Reserve_WithNegativeSeats_ThrowsBeforeCallingDependencies()
    {
        // Arrange: Doubles, deren Aufrufe sich prüfen lassen.
        var availability = new SeatAvailabilityStub(Code, 3);
        var repository = new ReservationRepositoryDouble(saveSucceeds: true);
        var sender = new ConfirmationSenderSpy();
        var service = new ReservationService(availability, repository, sender);

        // Act:     -1 Plätze reservieren.
        var exception = Record.Exception(() => service.Reserve(Email, Code, -1));

        // Assert:  ArgumentOutOfRangeException; keine Abhängigkeit wurde aufgerufen.
        var argumentException = Assert.IsType<ArgumentOutOfRangeException>(exception);
        Assert.Equal("seatCount", argumentException.ParamName);
        Assert.Equal(0, availability.AvailabilityRequests);
        Assert.Empty(repository.SaveAttempts);
        Assert.Empty(sender.SentConfirmations);
    }

    [Fact]
    public void Reserve_WithEmptyEventCode_ThrowsBeforeCallingDependencies()
    {
        // Arrange: Doubles, deren Aufrufe sich prüfen lassen.
        var availability = new SeatAvailabilityStub(Code, 3);
        var repository = new ReservationRepositoryDouble(saveSucceeds: true);
        var sender = new ConfirmationSenderSpy();
        var service = new ReservationService(availability, repository, sender);

        // Act:     Mit leerem Veranstaltungscode reservieren.
        var exception = Record.Exception(() => service.Reserve(Email, "", 1));

        // Assert:  ArgumentException; keine Abhängigkeit wurde aufgerufen.
        var argumentException = Assert.IsType<ArgumentException>(exception);
        Assert.Equal("eventCode", argumentException.ParamName);
        Assert.Equal(0, availability.AvailabilityRequests);
        Assert.Empty(repository.SaveAttempts);
        Assert.Empty(sender.SentConfirmations);
    }

    [Fact(Skip = "TODO: E-Mail-Adresse aus Leerzeichen testen")]
    public void Reserve_WithWhitespaceEmail_ThrowsBeforeCallingDependencies()
    {
        // Arrange: Handgeschriebene Doubles oder Moq gezielt auswählen.
        // Act:     Mit einer E-Mail-Adresse aus Leerzeichen reservieren.
        // Assert:  ArgumentException; keine Abhängigkeit wurde aufgerufen.
        throw new NotImplementedException();
    }

    // Aufgabe 3:
    // SeatAvailabilityStub gibt den Service ein vorgegebenen Wert während ConfirmationSenderSpy speichert, welche Reservierung der Service in ihn übergibt.
    // Der Test kann damit die Aufrufe nachher prüfen.

    [Fact]
    public void Reserve_WhenSavingFails_DoesNotSendConfirmation()
    {
        // Arrange: Stub mit genügend Plätzen, Repository liefert false, eigener Spy.
        var availability = new SeatAvailabilityStub(Code, 3);
        var repository = new ReservationRepositoryDouble(saveSucceeds: false);
        var sender = new ConfirmationSenderSpy();
        var service = new ReservationService(availability, repository, sender);

        // Act:     Reservierung versuchen.
        var result = service.Reserve(Email, Code, 2);

        // Assert:  Ergebnis ist false und der Spy hat keinen Aufruf aufgezeichnet.
        Assert.False(result);
        Assert.Single(repository.SaveAttempts);
        Assert.Empty(sender.SentConfirmations);
    }

    // Aufgabe 4:
    //

    [Fact]
    public void Reserve_WithTooFewSeats_DoesNotCallRepository()
    {
        // Arrange: Stub mit zu wenigen Plätzen und Mock<IReservationRepository>.
        var availability = new SeatAvailabilityStub(Code, 1);
        var repository = new Mock<IReservationRepository>();
        repository.Setup(r => r.TrySave(It.IsAny<Reservation>())).Returns(true);
        var sender = new ConfirmationSenderSpy();
        var service = new ReservationService(availability, repository.Object, sender);

        // Act:     Reservierung versuchen.
        var result = service.Reserve(Email, Code, 2);

        // Assert:  Verify(..., Times.Never()) für TrySave.
        Assert.False(result);
        repository.Verify(r => r.TrySave(It.IsAny<Reservation>()), Times.Never());
        Assert.Empty(sender.SentConfirmations);
    }

    [Fact(Skip = "Fortgeschrittene: ReserveBatch zuerst implementieren")]
    public void ReserveBatch_WhenCombinedSeatsExceedCapacity_DoesNotSaveOrSend()
    {
        // Arrange: Zwei Reservierungen für dieselbe Veranstaltung, die einzeln passen.
        // Act:     Nach Implementierung des Batch-Verfahrens beide zusammen reservieren.
        // Assert:  Ergebnis ist false; kein Batch-Speichern und keine Bestätigung.
        throw new NotImplementedException();
    }

    // Test Doubles
    // Stub
    private sealed class SeatAvailabilityStub(string eventCode, int availableSeats) : ISeatAvailability
    {
        public int AvailabilityRequests { get; private set; }

        public int GetAvailableSeats(string requestedEventCode)
        {
            AvailabilityRequests++;
            return requestedEventCode == eventCode ? availableSeats : 0;
        }
    }

    // Repository double
    private sealed class ReservationRepositoryDouble(bool saveSucceeds) : IReservationRepository
    {
        private readonly List<Reservation> _saveAttempts = [];

        public IReadOnlyList<Reservation> SaveAttempts => _saveAttempts;

        public bool TrySave(Reservation reservation)
        {
            _saveAttempts.Add(reservation);
            return saveSucceeds;
        }
    }

    // Spy
    private sealed class ConfirmationSenderSpy : IConfirmationSender
    {
        private readonly List<Reservation> _sentConfirmations = [];

        public IReadOnlyList<Reservation> SentConfirmations => _sentConfirmations;

        public void Send(Reservation reservation) => _sentConfirmations.Add(reservation);
    }
}
