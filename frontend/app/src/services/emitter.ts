import mitt from 'mitt'

type Events = {
  logoutRequired: { reason?: string }
}

export default mitt<Events>()
