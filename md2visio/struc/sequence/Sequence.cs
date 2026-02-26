using md2visio.Api;
using md2visio.struc.figure;
using md2visio.vsdx;
using md2visio.vsdx.@base;

namespace md2visio.struc.sequence
{
    internal class Sequence : Figure
    {
        public List<SeqParticipant> Participants { get; set; } = new List<SeqParticipant>();
        public List<SeqMessage> Messages { get; set; } = new List<SeqMessage>();
        public List<SeqActivation> Activations { get; set; } = new List<SeqActivation>();
        public List<SeqFragment> Fragments { get; set; } = new List<SeqFragment>();
        public List<SeqNote> Notes { get; set; } = new List<SeqNote>();
        public bool ShowSequenceNumbers { get; set; } = false;

        public Sequence()
        {
        }

        public SeqParticipant GetParticipant(string id)
        {
            var participant = Participants.FirstOrDefault(p => p.ID == id);
            if (participant == null)
            {
                participant = new SeqParticipant { ID = id, Label = id };
                Participants.Add(participant);
            }
            return participant;
        }

        public void AddMessage(string from, string to, string label, string arrowType)
        {
            var message = new SeqMessage
            {
                From = from,
                To = to,
                Label = label,
                ArrowType = arrowType
            };
            Messages.Add(message);

            // Ensure participants exist
            GetParticipant(from);
            GetParticipant(to);
        }

        public void AddActivation(string participantId, double startY)
        {
            var activation = new SeqActivation
            {
                ParticipantId = participantId,
                StartY = startY,
                EndY = startY // Will be updated on deactivate
            };
            Activations.Add(activation);
        }

        public void DeactivateLatest(string participantId, double endY)
        {
            var activation = Activations.LastOrDefault(a => a.ParticipantId == participantId && a.EndY == a.StartY);
            if (activation != null)
            {
                activation.EndY = endY;
            }
        }

        public override void ToVisio(string path, ConversionContext context, IVisioSession session)
        {
            new VBuilderSeq(this, context, session).Build(path);
        }
    }
}
