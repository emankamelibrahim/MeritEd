import { isAxiosError } from 'axios'

export function getErrorMessages(err: unknown): string[] {
  if (isAxiosError(err)) {
    if (!err.response) return ['Cannot reach the server. Is the API running?']
    const data = err.response.data
    if (data?.details?.length) return data.details
    if (data?.errors) return Object.values(data.errors).flat() as string[]
    if (data?.error) return [data.error]
  }
  return ['Something went wrong. Please try again.']
}