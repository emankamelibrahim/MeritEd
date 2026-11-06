export default function ErrorBox({ messages }: { messages: string[] }) {
  return (
    <ul role="alert" className="space-y-1 rounded-lg bg-danger/10 p-3 text-sm text-danger">
      {messages.map((m) => (
        <li key={m}>{m}</li>
      ))}
    </ul>
  )
}