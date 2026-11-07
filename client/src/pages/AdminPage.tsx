import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { usersApi } from '../api/users'
import ErrorBox from '../components/ErrorBox'
import { getErrorMessages } from '../utils/errors'
import { formatDate } from '../utils/format'

export default function AdminPage() {
  const queryClient = useQueryClient()
  const pending = useQuery({ queryKey: ['admin', 'pending'], queryFn: usersApi.pending })
  const approve = useMutation({
    mutationFn: (id: string) => usersApi.approve(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin', 'pending'] }),
  })

  return (
    <>
      <h1 className="font-heading text-3xl font-extrabold text-navy">Instructor approvals</h1>
      <div className="mt-6 max-w-3xl space-y-3">
        {pending.isLoading && <p className="text-muted">Loading…</p>}
        {pending.isError && <ErrorBox messages={getErrorMessages(pending.error)} />}
        {approve.isError && <ErrorBox messages={getErrorMessages(approve.error)} />}
        {pending.data?.length === 0 && (
          <p className="text-muted">No instructors are waiting for approval.</p>
        )}
        {pending.data?.map((p) => (
          <div key={p.id}
            className="flex items-center justify-between gap-4 rounded-2xl bg-surface p-4 shadow-sm">
            <div>
              <p className="font-medium text-ink">{p.displayName}</p>
              <p className="text-sm text-muted">
                {p.email} · registered {formatDate(p.createdAt)}
              </p>
            </div>
            <button
              onClick={() => approve.mutate(p.id)}
              disabled={approve.isPending && approve.variables === p.id}
              className="rounded-lg bg-navy px-4 py-2 text-sm font-semibold text-white hover:opacity-90 disabled:opacity-60"
            >
              Approve
            </button>
          </div>
        ))}
      </div>
    </>
  )
}