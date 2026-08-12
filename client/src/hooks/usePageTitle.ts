import { useEffect } from 'react'

function usePageTitle(title: string) {
  useEffect(function () {
    document.title = title + ' | Pantry & Plan'
  }, [title])
}

export default usePageTitle