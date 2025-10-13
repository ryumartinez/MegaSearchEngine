import { Outlet, createRootRoute } from '@tanstack/react-router'
import { TanStackRouterDevtoolsPanel } from '@tanstack/react-router-devtools'
import { TanStackDevtools } from '@tanstack/react-devtools'
import {ApplicationLayout} from "@/layouts/application-layout.tsx";
import {getEvents} from "@/data.ts";

let events = await getEvents()

export const Route = createRootRoute({
  component: () => (
    <>
      <ApplicationLayout events={events}>
        <Outlet />
        <TanStackDevtools
          config={{
            position: 'bottom-right',
          }}
          plugins={[
            {
              name: 'Tanstack Router',
              render: <TanStackRouterDevtoolsPanel />,
            },
          ]}
        />
      </ApplicationLayout>
    </>
  ),
})
