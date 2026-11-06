import type { InputHTMLAttributes } from 'react'

interface Props extends InputHTMLAttributes<HTMLInputElement> {
  label: string
}

export default function FormField({ label, ...rest }: Props) {
  return (
    <label className="block">
      <span className="mb-1 block text-sm font-medium text-ink">{label}</span>
      <input
        className="w-full rounded-lg border border-gray-300 bg-surface px-3 py-2 text-ink outline-none focus:border-gold focus:ring-2 focus:ring-gold/30"
        {...rest}
      />
    </label>
  )
}