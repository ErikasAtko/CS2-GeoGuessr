import type { MapDto } from '../types'

interface Props {
  maps: MapDto[]
  onStart: (mapCode?: string) => void
}

export default function StartScreen({ maps, onStart }: Props) {
  return (
    <div className="screen start-screen">
      <h1>CS2-GeoGuessr</h1>
      <p>A screenshot from somewhere on the map pops up - click where on the minimap you think it was taken.</p>
      <div className="map-list">
        <button onClick={() => onStart()}>Any map</button>
        {maps.map((map) => (
          <button key={map.code} onClick={() => onStart(map.code)}>
            {map.displayName}
          </button>
        ))}
      </div>
    </div>
  )
}
