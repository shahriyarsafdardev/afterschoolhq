import React from 'react';
import cs from 'classnames';
import { Box, Typography } from '@material-ui/core';
import { useSearchParams } from 'react-router-dom';

import useStyles from './styles';
import { statusIcons } from 'constants';

export default function SidebarSection(props) {
  const classes = useStyles();
  const { questionsGroups } = props;
  const [searchParams, setSearchParams] = useSearchParams();
  const sectionName = searchParams.get('section');

  const setSectionParam = name => {
    setSearchParams(`section=${encodeURIComponent(name)}`);
  };

  const isNavigationAllowed = sectionIndex => {
    const lastCompletedSectionId = localStorage.getItem('lastCompletedSectionId');

    return (
      lastCompletedSectionId &&
      questionsGroups[lastCompletedSectionId].Status === 'completed' &&
      sectionIndex <= Number(lastCompletedSectionId) + 1
    );
  };

  const handleNavigation = (name, index) => {
    const allowNavigation = isNavigationAllowed(index);

    if (allowNavigation) setSectionParam(name);
  };

  return (
    questionsGroups?.length && (
      <Box className={classes.sidebar}>
        <Typography className={classes.sidebarHeader} variant="h6">
          What's Next?
        </Typography>
        <Box className={classes.sectionsListWrapper}>
          {questionsGroups.map((section, index) => (
            <Box
              key={index}
              className={cs(
                classes.sectionItemWrapper,
                isNavigationAllowed(index) && classes.sectionItemWrapperHover
              )}
              onClick={() => handleNavigation(section.Name, index)}
            >
              <Typography className={classes.icon}>
                {sectionName === section.Name
                  ? statusIcons.currentlyEditing
                  : statusIcons[section.Status]}
              </Typography>
              <Typography className={classes.sectionName}>{section.Name}</Typography>
            </Box>
          ))}
        </Box>
        <Typography variant="body2" className={classes.scrollText}>
          Scroll for more
        </Typography>
      </Box>
    )
  );
}
