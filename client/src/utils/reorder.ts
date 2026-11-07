export function moved<T>(list: T[], index: number, direction: -1 | 1): T[] {
  const target = index + direction
  if (target < 0 || target >= list.length) return list
  const copy = [...list]
  const item = copy[index]
  copy[index] = copy[target]
  copy[target] = item
  return copy
}