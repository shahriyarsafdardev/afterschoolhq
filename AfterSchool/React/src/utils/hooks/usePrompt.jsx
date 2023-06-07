import { useNavigate } from 'react-router-dom';
import { useContext, useEffect, useCallback, useState } from 'react';
import { UNSAFE_NavigationContext as NavigationContext } from 'react-router-dom';

function useBlocker( blocker, when = true ) {
	const { navigator } = useContext( NavigationContext );

	useEffect( () => {
		if ( !when ) return;

		const unblock = navigator.block( ( tx ) => {
			const autoUnblockingTx = {
				...tx,
				retry() {
					unblock();
					tx.retry();
				},
			};

			blocker( autoUnblockingTx );
		} );

		return unblock;
	}, [ navigator, blocker, when ] );
}

export function usePrompt( when = true ) {
	const navigate = useNavigate();
    const [showPrompt, setShowPrompt] = useState(false);
    const [lastLocation, setLastLocation] = useState(null);
    const [confirmedNavigation, setConfirmedNavigation] = useState(false);

    const cancelNavigation = useCallback(() => {
        setShowPrompt(false);
    }, []);

    const handleBlockedNavigation = useCallback(
        (nextLocation) => {
            if (!confirmedNavigation && nextLocation.location !== window.location) {
                setShowPrompt(true);
                setLastLocation(nextLocation);
                return false;
            }
            return true;
        },
        [confirmedNavigation, window.location.href]
    );

    const confirmNavigation = useCallback(() => {
        setShowPrompt(false);
        setConfirmedNavigation(true);
    }, []);

    useEffect(() => {
        if (confirmedNavigation && lastLocation) {
            navigate(lastLocation.location);

            setConfirmedNavigation(false);
        }
    }, [confirmedNavigation, lastLocation]);

    useBlocker(handleBlockedNavigation, when);

    return [showPrompt, confirmNavigation, cancelNavigation];
}