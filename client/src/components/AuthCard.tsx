import type { ReactNode } from 'react'

interface Props {
  subtitle: string
  footer: ReactNode
  children: ReactNode
}

export default function AuthCard({ subtitle, footer, children }: Props) {
  return (
    <div className="flex min-h-screen items-center justify-center px-4">
      <div className="w-full max-w-md rounded-2xl bg-surface p-8 shadow-lg">
        <div className="mb-6 flex flex-col items-center">
          <img src="/logo.png" alt="MeritEd logo" className="h-16 w-16" />
          <h1 className="mt-2 font-heading text-3xl font-extrabold text-navy">
            Merit<span className="text-gold">Ed</span>
          </h1>
          <p className="mt-1 text-muted">{subtitle}</p>
        </div>
        {children}
        <p className="mt-6 text-center text-sm text-muted">{footer}</p>
      </div>
    </div>
  )
}