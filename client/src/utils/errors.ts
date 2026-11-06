import { isAxiosError } from 'axios'

export function getErrorMessages(err: unknown): string[] {
  if (isAxiosError(err)) {
    if (!err.response) return ['Cannot reach the server. Is the API running?']
    const { status, data } = err.response
    if (data?.details?.length) return data.details
    if (data?.errors) return Object.values(data.errors).flat() as string[]
    if (data?.error) return [data.error]
    if (status === 404) return ['Not found.']
    if (status === 403) return ["You don't have access to this."]
  }
  return ['Something went wrong. Please try again.']
}